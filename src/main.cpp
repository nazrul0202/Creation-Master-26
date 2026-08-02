#include "database_engine.h"
#include <windows.h>
#include <commctrl.h>
#include <shlobj.h>
#include <commdlg.h>
#include <windowsx.h>
#include <filesystem>
#include <memory>
#include <sstream>

#pragma comment(lib, "comctl32.lib")
#pragma comment(lib, "comdlg32.lib")

using namespace cm26;

namespace {
constexpr int IDM_OPEN = 40001, IDM_EXIT = 40002;
constexpr int ID_TABLES = 100, ID_ROWS = 101, ID_SELECTOR = 102, ID_STATUS = 103;
constexpr int ID_ROW = 104, ID_FIELD = 105, ID_VALUE = 106, ID_STAGE = 107, ID_SAVE = 108;
constexpr int ID_CATEGORY_BASE = 200;
constexpr int ID_MODULE_RECORDS = 320, ID_MODULE_FIELDS = 321, ID_RAW_VIEW = 322, ID_GENERIC_VIEW = 323, ID_APPLY_MODULE = 324;

HWND g_tables{}, g_rows{}, g_selector{}, g_status{}, g_row{}, g_field{}, g_value{}, g_stage{}, g_save{};
HWND g_moduleCaption{}, g_modulePicker{}, g_moduleFields{}, g_moduleInfo{}, g_rawView{}, g_genericView{}, g_formPanel{}, g_applyModule{};
DatabaseEngine g_engine;
std::unique_ptr<NativeDatabase> g_main, g_locale;
std::filesystem::path g_mainMetaPath, g_mainDbPath, g_localeDbPath;

struct TableRef { NativeDatabase* database; size_t index; std::wstring title; };
std::vector<TableRef> g_refs;
int g_selectedTable = -1;
int g_activeModule = -1;
std::wstring g_activeModuleName;

struct BoundControl { std::string field; HWND edit{}; };
std::vector<BoundControl> g_boundControls;

std::wstring wide(const std::string& s) { return std::wstring(s.begin(), s.end()); }
std::string narrow(const std::wstring& s) {
    if (s.empty()) return {};
    const int bytes = WideCharToMultiByte(CP_UTF8, 0, s.data(), static_cast<int>(s.size()), nullptr, 0, nullptr, nullptr);
    std::string result(static_cast<size_t>(bytes), '\0');
    WideCharToMultiByte(CP_UTF8, 0, s.data(), static_cast<int>(s.size()), result.data(), bytes, nullptr, nullptr);
    return result;
}

std::wstring cellText(const CellValue& value) {
    if (const auto integer = std::get_if<int>(&value)) return std::to_wstring(*integer);
    if (const auto decimal = std::get_if<float>(&value)) { std::wostringstream text; text << *decimal; return text.str(); }
    return wide(std::get<std::string>(value));
}

void setStatus(const std::wstring& text) { SetWindowTextW(g_status, text.c_str()); }

std::filesystem::path chooseFile(HWND window, const wchar_t* title, const wchar_t* filter) {
    wchar_t path[MAX_PATH]{};
    OPENFILENAMEW dialog{};
    dialog.lStructSize = sizeof(dialog);
    dialog.hwndOwner = window;
    dialog.lpstrTitle = title;
    dialog.lpstrFilter = filter;
    dialog.lpstrFile = path;
    dialog.nMaxFile = MAX_PATH;
    dialog.Flags = OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_HIDEREADONLY;
    return GetOpenFileNameW(&dialog) ? std::filesystem::path(path) : std::filesystem::path{};
}

void clearColumns() { while (ListView_DeleteColumn(g_rows, 0)) {} }

void showTable(int selected) {
    ListView_DeleteAllItems(g_rows);
    clearColumns();
    SendMessageW(g_field, CB_RESETCONTENT, 0, 0);
    g_selectedTable = selected;
    if (selected < 0 || selected >= static_cast<int>(g_refs.size())) return;

    const auto& ref = g_refs[selected];
    const auto& table = ref.database->tables[ref.index];
    for (size_t column = 0; column < table.columns.size(); ++column) {
        LVCOLUMNW header{};
        header.mask = LVCF_TEXT | LVCF_WIDTH;
        header.cx = 145;
        auto name = wide(table.columns[column].name);
        header.pszText = name.data();
        ListView_InsertColumn(g_rows, static_cast<int>(column), &header);
        SendMessageW(g_field, CB_ADDSTRING, 0, reinterpret_cast<LPARAM>(name.c_str()));
    }
    SendMessageW(g_field, CB_SETCURSEL, 0, 0);

    const size_t displayedRows = std::min<size_t>(table.rows.size(), 250);
    for (size_t row = 0; row < displayedRows; ++row) {
        for (size_t column = 0; column < table.columns.size(); ++column) {
            auto text = cellText(table.rows[row].values[column]);
            if (column == 0) {
                LVITEMW item{};
                item.mask = LVIF_TEXT;
                item.iItem = static_cast<int>(row);
                item.pszText = text.data();
                ListView_InsertItem(g_rows, &item);
            } else {
                ListView_SetItemText(g_rows, static_cast<int>(row), static_cast<int>(column), text.data());
            }
        }
    }
    setStatus(L"Loaded " + wide(table.name) + L" - " + std::to_wstring(table.rows.size()) +
              L" records; showing first " + std::to_wstring(displayedRows));
}

void fillCatalog() {
    ListBox_ResetContent(g_tables);
    SendMessageW(g_selector, CB_RESETCONTENT, 0, 0);
    g_refs.clear();
    const auto addTables = [](NativeDatabase* database, const wchar_t* prefix) {
        for (size_t i = 0; i < database->tables.size(); ++i) {
            const auto& table = database->tables[i];
            TableRef ref{ database, i, std::wstring(prefix) + L" / " + wide(table.name) + L" (" + std::to_wstring(table.rows.size()) + L")" };
            g_refs.push_back(ref);
            ListBox_AddString(g_tables, ref.title.c_str());
            SendMessageW(g_selector, CB_ADDSTRING, 0, reinterpret_cast<LPARAM>(ref.title.c_str()));
        }
    };
    addTables(g_main.get(), L"DB");
    addTables(g_locale.get(), L"LOC");
    ListBox_SetCurSel(g_tables, 0);
    SendMessageW(g_selector, CB_SETCURSEL, 0, 0);
    showTable(0);
}

void selectMappedTable(const std::string& tableName) {
    for (size_t index = 0; index < g_refs.size(); ++index) {
        const auto& table = g_refs[index].database->tables[g_refs[index].index];
        if (_stricmp(table.name.c_str(), tableName.c_str()) != 0) continue;
        ListBox_SetCurSel(g_tables, static_cast<int>(index));
        SendMessageW(g_selector, CB_SETCURSEL, static_cast<WPARAM>(index), 0);
        showTable(static_cast<int>(index));
        return;
    }
    setStatus(L"Mapping is not available in this FC26 database set: " + wide(tableName));
}

void setModuleControlsVisible(bool visible) {
    const int show = visible ? SW_SHOW : SW_HIDE;
    ShowWindow(g_moduleCaption, show);
    ShowWindow(g_modulePicker, show);
    ShowWindow(g_moduleFields, show);
    ShowWindow(g_moduleInfo, show);
    ShowWindow(g_rawView, show);
    ShowWindow(g_genericView, show);
}

std::wstring recordLabel(const NativeTable& table, size_t row) {
    if (row >= table.rows.size()) return L"";
    const auto& values = table.rows[row].values;
    const size_t limit = std::min<size_t>(table.columns.size(), 4);
    std::wstring label = std::to_wstring(row) + L"  ";
    for (size_t i = 0; i < limit; ++i) {
        const auto value = cellText(values[i]);
        if (value.empty() || value == L"0") continue;
        if (label.size() > 4) label += L" - ";
        label += value;
        if (label.size() > 72) break;
    }
    return label;
}

void showModuleRecord(int row) {
    if (g_selectedTable < 0 || g_selectedTable >= static_cast<int>(g_refs.size())) return;
    const auto& ref = g_refs[g_selectedTable];
    const auto& table = ref.database->tables[ref.index];
    if (row < 0 || row >= static_cast<int>(table.rows.size())) return;
    ListView_DeleteAllItems(g_moduleFields);
    for (size_t col = 0; col < table.columns.size(); ++col) {
        auto field = wide(table.columns[col].name);
        auto value = cellText(table.rows[row].values[col]);
        LVITEMW item{};
        item.mask = LVIF_TEXT;
        item.iItem = static_cast<int>(col);
        item.pszText = field.data();
        ListView_InsertItem(g_moduleFields, &item);
        ListView_SetItemText(g_moduleFields, static_cast<int>(col), 1, value.data());
    }
    SetWindowTextW(g_moduleInfo, (L"Generic mapping: " + g_activeModuleName + L"  |  record " + std::to_wstring(row) +
        L"\r\n\r\nSelect any field below, then edit it using the bottom value editor and click Stage Value.\r\n"
        L"Raw Database remains available for complete table inspection.\r\n\r\n"
        L"Asset viewers (logos, kits, faces, 3D) require a separate FC26 asset-archive reader and are intentionally not simulated here.").c_str());
}

void showCm16Module(const std::wstring& moduleName, const std::string& tableName) {
    selectMappedTable(tableName);
    if (g_selectedTable < 0 || g_selectedTable >= static_cast<int>(g_refs.size())) return;
    g_activeModuleName = moduleName;
    g_activeModule = g_selectedTable;
    const auto& ref = g_refs[g_selectedTable];
    const auto& table = ref.database->tables[ref.index];
    SetWindowTextW(g_moduleCaption, (moduleName + L" Editor    Generic   |   Raw Database").c_str());
    SendMessageW(g_modulePicker, CB_RESETCONTENT, 0, 0);
    const size_t limit = std::min<size_t>(table.rows.size(), 1000);
    for (size_t row = 0; row < limit; ++row) {
        const auto label = recordLabel(table, row);
        SendMessageW(g_modulePicker, CB_ADDSTRING, 0, reinterpret_cast<LPARAM>(label.c_str()));
    }
    if (limit) SendMessageW(g_modulePicker, CB_SETCURSEL, 0, 0);
    setModuleControlsVisible(true);
    ShowWindow(g_rows, SW_HIDE);
    ShowWindow(g_tables, SW_HIDE);
    ShowWindow(g_selector, SW_HIDE);
    showModuleRecord(0);
    setStatus(L"CM16-style " + moduleName + L" editor loaded from table " + wide(table.name) + L". " +
              std::to_wstring(table.rows.size()) + L" records available.");
}

void showRawDatabase() {
    setModuleControlsVisible(false);
    ShowWindow(g_rows, SW_SHOW);
    ShowWindow(g_tables, SW_SHOW);
    ShowWindow(g_selector, SW_SHOW);
    if (g_selectedTable >= 0) showTable(g_selectedTable);
    setStatus(L"Raw Database browser — use this for full table structure and values.");
}

void openSet(HWND window) {
    try {
        const auto meta = chooseFile(window, L"Open XML Descriptor File", L"XML files (*.xml)\0*.xml\0");
        if (meta.empty()) return;
        if (_wcsicmp(meta.filename().c_str(), L"fifa_ng_db-meta.xml") != 0) throw std::runtime_error("Step 1 must be fifa_ng_db-meta.xml.");
        const auto database = chooseFile(window, L"Open Database File", L"Database files (*.db)\0*.db\0");
        if (database.empty()) return;
        if (_wcsicmp(database.filename().c_str(), L"fifa_ng_db.db") != 0) throw std::runtime_error("Step 2 must be fifa_ng_db.db.");
        const auto locale = chooseFile(window, L"Open Language Database", L"Database files (*.db)\0*.db\0");
        if (locale.empty()) return;
        if (_wcsicmp(locale.filename().c_str(), L"eng_us.db") != 0) throw std::runtime_error("Step 3 must be eng_us.db.");
        setStatus(L"1/3 Reading fifa_ng_db-meta.xml..."); UpdateWindow(window);
        setStatus(L"2/3 Reading fifa_ng_db.db..."); UpdateWindow(window);
        g_main = std::make_unique<NativeDatabase>(g_engine.readT3db(meta, database));
        setStatus(L"3/3 Decrypting eng_us.db with built-in locale descriptor..."); UpdateWindow(window);
        g_locale = std::make_unique<NativeDatabase>(g_engine.readT3db(L"", locale, true));
        g_mainMetaPath = meta;
        g_mainDbPath = database;
        g_localeDbPath = locale;
        setStatus(L"Building table browser..."); UpdateWindow(window);
        fillCatalog();
    } catch (const std::exception& error) {
        const auto message = wide(error.what());
        setStatus(L"Load failed: " + message);
        MessageBoxW(window, message.c_str(), L"CM26 Database Engine", MB_ICONERROR);
    }
}

std::wstring controlText(HWND control) {
    const int length = GetWindowTextLengthW(control);
    std::wstring value(static_cast<size_t>(length) + 1, L'\0');
    GetWindowTextW(control, value.data(), length + 1);
    value.resize(static_cast<size_t>(length));
    return value;
}

void stageValue(HWND window) {
    if (g_selectedTable < 0) return;
    try {
        const int row = _wtoi(controlText(g_row).c_str());
        const int selection = static_cast<int>(SendMessageW(g_field, CB_GETCURSEL, 0, 0));
        if (selection < 0) throw std::runtime_error("Select a field.");
        wchar_t field[256]{};
        SendMessageW(g_field, CB_GETLBTEXT, selection, reinterpret_cast<LPARAM>(field));
        const auto value = controlText(g_value);
        const auto& ref = g_refs[g_selectedTable];
        const auto result = g_engine.stageEdit(*ref.database, ref.database->tables[ref.index].name, row, narrow(field), narrow(value));
        if (!result.success) throw std::runtime_error(result.message);
        showTable(g_selectedTable);
        setStatus(L"Staged: row " + std::to_wstring(row) + L", " + field + L" = " + value + L". Save Copy to write it.");
    } catch (const std::exception& error) {
        MessageBoxW(window, wide(error.what()).c_str(), L"CM26 Value Editor", MB_ICONERROR);
    }
}

void saveAll(HWND window) {
    if (!g_main || !g_locale || g_mainMetaPath.empty()) return;
    BROWSEINFOW browse{};
    browse.hwndOwner = window;
    browse.lpszTitle = L"Select output folder for the CM26 database set";
    const auto itemId = SHBrowseForFolderW(&browse);
    if (!itemId) return;
    wchar_t folder[MAX_PATH]{};
    if (!SHGetPathFromIDListW(itemId, folder)) { CoTaskMemFree(itemId); return; }
    CoTaskMemFree(itemId);
    try {
        const std::filesystem::path output(folder);
        const auto outputMeta = output / L"fifa_ng_db-meta.xml";
        const auto outputMain = output / L"fifa_ng_db.db";
        const auto outputLocale = output / L"eng_us.db";
        setStatus(L"Saving fifa_ng_db-meta.xml..."); UpdateWindow(window);
        std::filesystem::copy_file(g_mainMetaPath, outputMeta, std::filesystem::copy_options::overwrite_existing);
        setStatus(L"Saving fifa_ng_db.db..."); UpdateWindow(window);
        g_engine.saveT3dbCopy(*g_main, outputMain);
        setStatus(L"Saving and encrypting eng_us.db..."); UpdateWindow(window);
        g_engine.saveT3dbCopy(*g_locale, outputLocale);
        setStatus(L"Verifying saved database set..."); UpdateWindow(window);
        const auto mainCheck = g_engine.readT3db(outputMeta, outputMain);
        const auto localeCheck = g_engine.readT3db(L"", outputLocale, true);
        if (mainCheck.tables.empty() || localeCheck.tables.empty()) throw std::runtime_error("Saved set reload verification failed");
        setStatus(L"Save All complete: fifa_ng_db-meta.xml, fifa_ng_db.db, eng_us.db");
        MessageBoxW(window, L"Saved and reload-verified all three database files. The original selected files were not changed.", L"CM26 Save All", MB_ICONINFORMATION);
    } catch (const std::exception& error) {
        MessageBoxW(window, wide(error.what()).c_str(), L"CM26 Save All", MB_ICONERROR);
    }
}

void layout(HWND window) {
    RECT client{};
    GetClientRect(window, &client);
    const int toolbarBottom = 103;
    const int footerY = client.bottom - 104;
    MoveWindow(g_selector, 8, toolbarBottom + 7, 410, 25, TRUE);
    MoveWindow(g_tables, 8, toolbarBottom + 37, 410, footerY - toolbarBottom - 45, TRUE);
    MoveWindow(g_rows, 427, toolbarBottom + 7, client.right - 435, footerY - toolbarBottom - 15, TRUE);
    MoveWindow(g_moduleCaption, 8, toolbarBottom + 7, client.right - 16, 27, TRUE);
    MoveWindow(g_modulePicker, 8, toolbarBottom + 41, 380, 300, TRUE);
    MoveWindow(g_rawView, 397, toolbarBottom + 41, 100, 24, TRUE);
    MoveWindow(g_genericView, 504, toolbarBottom + 41, 100, 24, TRUE);
    MoveWindow(g_moduleFields, 8, toolbarBottom + 72, 600, footerY - toolbarBottom - 80, TRUE);
    MoveWindow(g_moduleInfo, 620, toolbarBottom + 72, client.right - 628, footerY - toolbarBottom - 80, TRUE);
    MoveWindow(g_row, 464, footerY, 64, 23, TRUE);
    MoveWindow(g_field, 582, footerY, 190, 250, TRUE);
    MoveWindow(g_value, 824, footerY, 260, 23, TRUE);
    MoveWindow(g_stage, 1095, footerY, 110, 23, TRUE);
    MoveWindow(g_save, 1215, footerY, 130, 23, TRUE);
    MoveWindow(g_status, 8, client.bottom - 31, client.right - 16, 22, TRUE);
}

LRESULT CALLBACK windowProc(HWND window, UINT message, WPARAM wParam, LPARAM lParam) {
    switch (message) {
    case WM_CREATE: {
        HMENU mainMenu = CreateMenu();
        HMENU fileMenu = CreatePopupMenu();
        AppendMenuW(fileMenu, MF_STRING, IDM_OPEN, L"Load FC 26 Database Files...");
        AppendMenuW(fileMenu, MF_SEPARATOR, 0, nullptr);
        AppendMenuW(fileMenu, MF_STRING, IDM_EXIT, L"Exit");
        AppendMenuW(mainMenu, MF_POPUP, reinterpret_cast<UINT_PTR>(fileMenu), L"File");
        AppendMenuW(mainMenu, MF_STRING, 0, L"Tools");
        AppendMenuW(mainMenu, MF_STRING, 0, L"Patch");
        AppendMenuW(mainMenu, MF_STRING, 0, L"Online Update");
        AppendMenuW(mainMenu, MF_STRING, 0, L"Help");
        SetMenu(window, mainMenu);

        const wchar_t* categories[] = { L"Globe", L"Country", L"League", L"Team", L"Player", L"Kit", L"Ball", L"Boots", L"Manager", L"Formation", L"Stadium", L"Audio" };
        for (int i = 0; i < 12; ++i) {
            CreateWindowW(L"BUTTON", categories[i], WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON, 8 + i * 76, 27, 70, 53, window, reinterpret_cast<HMENU>(static_cast<INT_PTR>(ID_CATEGORY_BASE + i)), GetModuleHandleW(nullptr), nullptr);
        }
        g_selector = CreateWindowW(L"COMBOBOX", L"", WS_CHILD | WS_VISIBLE | CBS_DROPDOWNLIST | WS_VSCROLL, 8, 101, 410, 300, window, reinterpret_cast<HMENU>(static_cast<INT_PTR>(ID_SELECTOR)), GetModuleHandleW(nullptr), nullptr);
        g_tables = CreateWindowW(L"LISTBOX", L"", WS_CHILD | WS_VISIBLE | WS_BORDER | LBS_NOTIFY | WS_VSCROLL | LBS_NOINTEGRALHEIGHT, 8, 131, 410, 450, window, reinterpret_cast<HMENU>(static_cast<INT_PTR>(ID_TABLES)), GetModuleHandleW(nullptr), nullptr);
        g_rows = CreateWindowW(WC_LISTVIEWW, L"", WS_CHILD | WS_VISIBLE | WS_BORDER | LVS_REPORT | LVS_SINGLESEL, 427, 101, 600, 480, window, reinterpret_cast<HMENU>(static_cast<INT_PTR>(ID_ROWS)), GetModuleHandleW(nullptr), nullptr);
        ListView_SetExtendedListViewStyle(g_rows, LVS_EX_FULLROWSELECT | LVS_EX_GRIDLINES | LVS_EX_DOUBLEBUFFER);
        g_moduleCaption = CreateWindowW(L"STATIC", L"", WS_CHILD | SS_CENTERIMAGE, 8, 110, 800, 27, window, nullptr, GetModuleHandleW(nullptr), nullptr);
        g_modulePicker = CreateWindowW(L"COMBOBOX", L"", WS_CHILD | CBS_DROPDOWNLIST | WS_VSCROLL, 8, 145, 380, 300, window, reinterpret_cast<HMENU>(static_cast<INT_PTR>(ID_MODULE_RECORDS)), GetModuleHandleW(nullptr), nullptr);
        g_rawView = CreateWindowW(L"BUTTON", L"Raw Database", WS_CHILD | BS_PUSHBUTTON, 397, 145, 100, 24, window, reinterpret_cast<HMENU>(static_cast<INT_PTR>(ID_RAW_VIEW)), GetModuleHandleW(nullptr), nullptr);
        g_genericView = CreateWindowW(L"BUTTON", L"Generic", WS_CHILD | BS_PUSHBUTTON, 504, 145, 100, 24, window, reinterpret_cast<HMENU>(static_cast<INT_PTR>(ID_GENERIC_VIEW)), GetModuleHandleW(nullptr), nullptr);
        g_moduleFields = CreateWindowW(WC_LISTVIEWW, L"", WS_CHILD | WS_BORDER | LVS_REPORT | LVS_SINGLESEL, 8, 176, 600, 480, window, reinterpret_cast<HMENU>(static_cast<INT_PTR>(ID_MODULE_FIELDS)), GetModuleHandleW(nullptr), nullptr);
        ListView_SetExtendedListViewStyle(g_moduleFields, LVS_EX_FULLROWSELECT | LVS_EX_GRIDLINES | LVS_EX_DOUBLEBUFFER);
        for (int column = 0; column < 2; ++column) {
            LVCOLUMNW header{}; header.mask = LVCF_TEXT | LVCF_WIDTH;
            header.cx = column ? 350 : 230;
            header.pszText = const_cast<wchar_t*>(column ? L"Value" : L"Field");
            ListView_InsertColumn(g_moduleFields, column, &header);
        }
        g_moduleInfo = CreateWindowW(L"EDIT", L"", WS_CHILD | WS_BORDER | ES_MULTILINE | ES_READONLY | WS_VSCROLL, 620, 176, 600, 480, window, nullptr, GetModuleHandleW(nullptr), nullptr);
        setModuleControlsVisible(false);
        CreateWindowW(L"STATIC", L"Row", WS_CHILD | WS_VISIBLE, 427, 700, 33, 22, window, nullptr, GetModuleHandleW(nullptr), nullptr);
        g_row = CreateWindowW(L"EDIT", L"0", WS_CHILD | WS_VISIBLE | WS_BORDER | ES_NUMBER, 464, 700, 64, 23, window, reinterpret_cast<HMENU>(static_cast<INT_PTR>(ID_ROW)), GetModuleHandleW(nullptr), nullptr);
        CreateWindowW(L"STATIC", L"Field", WS_CHILD | WS_VISIBLE, 538, 700, 40, 22, window, nullptr, GetModuleHandleW(nullptr), nullptr);
        g_field = CreateWindowW(L"COMBOBOX", L"", WS_CHILD | WS_VISIBLE | CBS_DROPDOWNLIST | WS_VSCROLL, 582, 700, 190, 250, window, reinterpret_cast<HMENU>(static_cast<INT_PTR>(ID_FIELD)), GetModuleHandleW(nullptr), nullptr);
        CreateWindowW(L"STATIC", L"Value", WS_CHILD | WS_VISIBLE, 778, 700, 44, 22, window, nullptr, GetModuleHandleW(nullptr), nullptr);
        g_value = CreateWindowW(L"EDIT", L"", WS_CHILD | WS_VISIBLE | WS_BORDER | ES_AUTOHSCROLL, 824, 700, 260, 23, window, reinterpret_cast<HMENU>(static_cast<INT_PTR>(ID_VALUE)), GetModuleHandleW(nullptr), nullptr);
        g_stage = CreateWindowW(L"BUTTON", L"Stage Value", WS_CHILD | WS_VISIBLE, 1095, 700, 110, 23, window, reinterpret_cast<HMENU>(static_cast<INT_PTR>(ID_STAGE)), GetModuleHandleW(nullptr), nullptr);
        g_save = CreateWindowW(L"BUTTON", L"Save All...", WS_CHILD | WS_VISIBLE, 1215, 700, 130, 23, window, reinterpret_cast<HMENU>(static_cast<INT_PTR>(ID_SAVE)), GetModuleHandleW(nullptr), nullptr);
        g_status = CreateWindowW(L"STATIC", L"Ready - Open FC 26 Database Set", WS_CHILD | WS_VISIBLE, 8, 750, 800, 22, window, reinterpret_cast<HMENU>(static_cast<INT_PTR>(ID_STATUS)), GetModuleHandleW(nullptr), nullptr);
        layout(window);
        return 0;
    }
    case WM_SIZE: layout(window); return 0;
    case WM_NOTIFY: {
        const auto* notification = reinterpret_cast<LPNMHDR>(lParam);
        if (notification->idFrom == ID_MODULE_FIELDS && notification->code == LVN_ITEMCHANGED) {
            const auto* change = reinterpret_cast<LPNMLISTVIEW>(lParam);
            if (!(change->uNewState & LVIS_SELECTED) || change->iItem < 0) return 0;
            wchar_t field[256]{};
            ListView_GetItemText(g_moduleFields, change->iItem, 0, field, 256);
            wchar_t value[1024]{};
            ListView_GetItemText(g_moduleFields, change->iItem, 1, value, 1024);
            SetWindowTextW(g_row, std::to_wstring(SendMessageW(g_modulePicker, CB_GETCURSEL, 0, 0)).c_str());
            const int count = static_cast<int>(SendMessageW(g_field, CB_GETCOUNT, 0, 0));
            for (int i = 0; i < count; ++i) {
                wchar_t option[256]{};
                SendMessageW(g_field, CB_GETLBTEXT, i, reinterpret_cast<LPARAM>(option));
                if (_wcsicmp(option, field) == 0) { SendMessageW(g_field, CB_SETCURSEL, i, 0); break; }
            }
            SetWindowTextW(g_value, value);
            return 0;
        }
        break;
    }
    case WM_COMMAND:
        if (LOWORD(wParam) == IDM_OPEN) { openSet(window); return 0; }
        if (LOWORD(wParam) == IDM_EXIT) { DestroyWindow(window); return 0; }
        if (LOWORD(wParam) == ID_STAGE) { stageValue(window); return 0; }
        if (LOWORD(wParam) == ID_SAVE) { saveAll(window); return 0; }
        if (LOWORD(wParam) == ID_RAW_VIEW) { showRawDatabase(); return 0; }
        if (LOWORD(wParam) == ID_GENERIC_VIEW) { return 0; }
        if (LOWORD(wParam) == ID_MODULE_RECORDS && HIWORD(wParam) == CBN_SELCHANGE) {
            showModuleRecord(static_cast<int>(SendMessageW(g_modulePicker, CB_GETCURSEL, 0, 0)));
            return 0;
        }
        if (LOWORD(wParam) == ID_MODULE_FIELDS && HIWORD(wParam) == LBN_SELCHANGE) return 0;
        if (LOWORD(wParam) >= ID_CATEGORY_BASE && LOWORD(wParam) < ID_CATEGORY_BASE + 12) {
            static const char* mappedTables[] = { "", "nations", "leagues", "teams", "players", "teamkits", "competitionballs", "footwear", "manager", "formations", "stadiums", "" };
            const char* table = mappedTables[LOWORD(wParam) - ID_CATEGORY_BASE];
            static const wchar_t* mappedModules[] = { L"Database", L"Country", L"League", L"Team", L"Player", L"Kit", L"Ball", L"Boots", L"Manager", L"Formation", L"Stadium", L"Audio" };
            if (LOWORD(wParam) == ID_CATEGORY_BASE) { showRawDatabase(); return 0; }
            if (table[0]) showCm16Module(mappedModules[LOWORD(wParam) - ID_CATEGORY_BASE], table);
            else setStatus(L"This CM16-style module needs additional FC26 asset mapping and is not enabled yet.");
            return 0;
        }
        if (LOWORD(wParam) == ID_TABLES && HIWORD(wParam) == LBN_SELCHANGE) {
            const int selected = ListBox_GetCurSel(g_tables);
            SendMessageW(g_selector, CB_SETCURSEL, selected, 0);
            showTable(selected);
            return 0;
        }
        if (LOWORD(wParam) == ID_SELECTOR && HIWORD(wParam) == CBN_SELCHANGE) {
            const int selected = static_cast<int>(SendMessageW(g_selector, CB_GETCURSEL, 0, 0));
            ListBox_SetCurSel(g_tables, selected);
            showTable(selected);
            return 0;
        }
        return 0;
    case WM_DESTROY: PostQuitMessage(0); return 0;
    }
    return DefWindowProcW(window, message, wParam, lParam);
}
}

int WINAPI wWinMain(HINSTANCE instance, HINSTANCE, PWSTR, int) {
    INITCOMMONCONTROLSEX controls{ sizeof(controls), ICC_LISTVIEW_CLASSES };
    InitCommonControlsEx(&controls);
    WNDCLASSW windowClass{};
    windowClass.lpfnWndProc = windowProc;
    windowClass.hInstance = instance;
    windowClass.hCursor = LoadCursor(nullptr, IDC_ARROW);
    windowClass.hbrBackground = CreateSolidBrush(RGB(135, 206, 235));
    windowClass.lpszClassName = L"CM26Classic";
    RegisterClassW(&windowClass);
    CreateWindowExW(0, windowClass.lpszClassName, L"CM26 - Creation Master 26", WS_OVERLAPPEDWINDOW | WS_VISIBLE, 60, 50, 1400, 900, nullptr, nullptr, instance, nullptr);
    MSG message{};
    while (GetMessageW(&message, nullptr, 0, 0)) { TranslateMessage(&message); DispatchMessageW(&message); }
    return 0;
}
