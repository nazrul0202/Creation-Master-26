// CM26.EngineBridge — managed (C++/CLI) façade over the PROTECTED native FC26 engine.
// This file contains NO database-format logic. It only adapts the native cm26::DatabaseEngine
// (compiled unchanged from src/database_engine.cpp) to managed DTOs consumed by CM26.Application.
#include "database_engine.h"

#using <System.dll>
#include <vcclr.h>
using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

namespace CM26 { namespace EngineBridge {

    // Internal native helpers shared by the façade (not exposed to managed callers).
    namespace detail {
        inline std::wstring Wide(String^ s) {
            if (s == nullptr) return std::wstring();
            pin_ptr<const wchar_t> p = PtrToStringChars(s);
            return std::wstring(p, s->Length);
        }
        inline std::string Narrow(String^ s) {
            if (s == nullptr) return std::string();
            array<System::Byte>^ bytes = System::Text::Encoding::UTF8->GetBytes(s);
            if (bytes->Length == 0) return std::string();
            pin_ptr<System::Byte> bp = &bytes[0];
            return std::string(reinterpret_cast<const char*>(bp), bytes->Length);
        }
        inline String^ CellText(const cm26::CellValue& v) {
            if (const auto i = std::get_if<int>(&v)) return i->ToString();
            if (const auto f = std::get_if<float>(&v)) return f->ToString(System::Globalization::CultureInfo::InvariantCulture);
            const auto& s = std::get<std::string>(v);
            if (s.empty()) return String::Empty;
            array<System::Byte>^ bytes = gcnew array<System::Byte>((int)s.size());
            for (int i = 0; i < (int)s.size(); ++i) bytes[i] = (System::Byte)s[(size_t)i];
            return System::Text::Encoding::UTF8->GetString(bytes);
        }
        // Raw bytes of a string cell (read-only). Lets managed code choose the correct codepage
        // (e.g. CP1252 for playernames) instead of assuming UTF-8.
        inline array<System::Byte>^ CellBytes(const cm26::CellValue& v) {
            const auto s = std::get_if<std::string>(&v);
            if (s == nullptr || s->empty()) return gcnew array<System::Byte>(0);
            array<System::Byte>^ bytes = gcnew array<System::Byte>((int)s->size());
            for (int i = 0; i < (int)s->size(); ++i) bytes[i] = (System::Byte)(*s)[(size_t)i];
            return bytes;
        }
        inline cm26::NativeTable* FindTable(cm26::NativeDatabase& db, const std::string& name, int& index) {
            auto eq = [](const std::string& a, const std::string& b) {
                return a.size() == b.size() && std::equal(a.begin(), a.end(), b.begin(),
                    [](unsigned char x, unsigned char y) { return std::tolower(x) == std::tolower(y); });
            };
            for (size_t i = 0; i < db.tables.size(); ++i) {
                if (eq(db.tables[i].name, name) || eq(db.tables[i].shortName, name)) { index = (int)i; return &db.tables[i]; }
            }
            index = -1; return nullptr;
        }
    }
    using namespace detail;

    public enum class LoadStateKind { Success = 0, MissingFile = 1, WrongKeyOrIv = 2, UnsupportedOrCorrupt = 3 };
    public enum class FieldKind { String = 0, Integer = 3, Float = 4, ShortCompressedString = 13, LongCompressedString = 14 };

    public ref class ColumnInfo {
    public:
        property String^ Name;
        property String^ ShortName;
        property FieldKind Kind;
        property int BitOffset;
        property int Depth;
        property int RangeLow;
        property int RangeHigh;
        property bool IsWritable; // engine can stage/write this kind
    };

    public ref class RowData {
    private:
        int _index;
        List<String^>^ _values = gcnew List<String^>();
    public:
        property int Index { int get() { return _index; } void set(int v) { _index = v; } }
        property List<String^>^ Values { List<String^>^ get() { return _values; } }
    };

    public ref class TableInfo {
    private:
        String^ _name; String^ _short; int _rows;
        List<ColumnInfo^>^ _columns = gcnew List<ColumnInfo^>();
    public:
        property String^ Name { String^ get() { return _name; } void set(String^ v) { _name = v; } }
        property String^ ShortName { String^ get() { return _short; } void set(String^ v) { _short = v; } }
        property int RowCount { int get() { return _rows; } void set(int v) { _rows = v; } }
        property List<ColumnInfo^>^ Columns { List<ColumnInfo^>^ get() { return _columns; } }
    };

    public ref class LoadSummary {
    public:
        property LoadStateKind State;
        property String^ Message;
        property int TableCount;
        property String^ MetaPath;
        property String^ DatabasePath;
        property String^ LocalePath;
    };

    public ref class EditOutcome {
    public:
        property bool Success;
        property String^ Message;
    };

    // Owns one loaded native database (main or locale) and stages edits in memory.
    // All reads/writes go through the protected native engine; originals are never modified.
    public ref class NativeDatabaseHandle {
    internal:
        cm26::NativeDatabase* _db;
        bool _isLocale;
        String^ _sourcePath;
        String^ _metaPath;

    public:
        static String^ Managed(const std::string& s) {
            if (s.empty()) return String::Empty;
            array<System::Byte>^ bytes = gcnew array<System::Byte>((int)s.size());
            for (int i = 0; i < (int)s.size(); ++i) bytes[i] = (System::Byte)s[(size_t)i];
            return System::Text::Encoding::UTF8->GetString(bytes);
        }
        static String^ Managed(const std::wstring& s) { return gcnew String(s.c_str()); }
        NativeDatabaseHandle(cm26::NativeDatabase* db, bool isLocale, String^ sourcePath, String^ metaPath)
            : _db(db), _isLocale(isLocale), _sourcePath(sourcePath), _metaPath(metaPath) {}
        ~NativeDatabaseHandle() { this->!NativeDatabaseHandle(); }
        !NativeDatabaseHandle() { if (_db) { delete _db; _db = nullptr; } }

        property bool IsLocale { bool get() { return _isLocale; } }
        property String^ SourcePath { String^ get() { return _sourcePath; } }
        property String^ MetaPath { String^ get() { return _metaPath; } }

        List<TableInfo^>^ GetTables();
        RowData^ GetRow(String^ tableName, int rowIndex);
        // Raw bytes of a single string cell (read-only), for codepage-specific decoding.
        array<System::Byte>^ GetCellBytes(String^ tableName, int rowIndex, String^ fieldName);
        int GetRowCount(String^ tableName);
    };

    public ref class EngineSession {
    private:
        cm26::DatabaseEngine* _engine;
        NativeDatabaseHandle^ _main;
        NativeDatabaseHandle^ _locale;
        String^ _metaPath; String^ _dbPath; String^ _localePath;

    public:
        EngineSession() { _engine = new cm26::DatabaseEngine(); _main = nullptr; _locale = nullptr; }
        ~EngineSession() { this->!EngineSession(); }
        !EngineSession() {
            if (_main) { delete _main; _main = nullptr; }
            if (_locale) { delete _locale; _locale = nullptr; }
            if (_engine) { delete _engine; _engine = nullptr; }
        }

        property bool IsLoaded { bool get() { return _main != nullptr && _locale != nullptr; } }
        property String^ DatabasePath { String^ get() { return _dbPath; } }
        property String^ LocalePath { String^ get() { return _localePath; } }
        property String^ MetaPath { String^ get() { return _metaPath; } }
        property NativeDatabaseHandle^ Main { NativeDatabaseHandle^ get() { return _main; } }
        property NativeDatabaseHandle^ Locale { NativeDatabaseHandle^ get() { return _locale; } }

        // Validate a folder that must contain fifa_ng_db-meta.xml, fifa_ng_db.db, eng_us.db.
        LoadSummary^ ValidateFolder(String^ folder) {
            auto result = _engine->loadFolder(Wide(folder));
            auto summary = gcnew LoadSummary();
            summary->State = (LoadStateKind)(int)result.state;
            summary->Message = NativeDatabaseHandle::Managed(result.message);
            summary->TableCount = (int)result.tables.size();
            summary->MetaPath = NativeDatabaseHandle::Managed(result.meta.wstring());
            summary->DatabasePath = NativeDatabaseHandle::Managed(result.database.wstring());
            summary->LocalePath = NativeDatabaseHandle::Managed(result.localization.wstring());
            return summary;
        }

        // Load main + locale fully into memory. Throws on failure (caller catches).
        void Load(String^ metaPath, String^ databasePath, String^ localePath) {
            std::wstring meta = Wide(metaPath), dbp = Wide(databasePath), loc = Wide(localePath);
            cm26::NativeDatabase* mainDb = nullptr;
            try {
                mainDb = new cm26::NativeDatabase(_engine->readT3db(meta, dbp, false));
                cm26::NativeDatabase* locDb = new cm26::NativeDatabase(_engine->readT3db(L"", loc, true));
                if (_main) { delete _main; } if (_locale) { delete _locale; }
                _main = gcnew NativeDatabaseHandle(mainDb, false, databasePath, metaPath);
                _locale = gcnew NativeDatabaseHandle(locDb, true, localePath, "");
                _metaPath = metaPath; _dbPath = databasePath; _localePath = localePath;
            } catch (const std::exception& ex) {
                delete mainDb;
                throw gcnew InvalidOperationException(
                    "Native database load failed: " + gcnew String(ex.what()));
            } catch (...) {
                delete mainDb;
                throw gcnew InvalidOperationException("Native database load failed with an unknown engine error.");
            }
        }

        // Stage one validated edit. Returns engine outcome; never writes bytes.
        EditOutcome^ StageEdit(bool locale, String^ tableName, int rowIndex, String^ fieldName, String^ value) {
            auto outcome = gcnew EditOutcome();
            NativeDatabaseHandle^ handle = locale ? _locale : _main;
            if (handle == nullptr) { outcome->Success = false; outcome->Message = "Database not loaded"; return outcome; }
            auto r = _engine->stageEdit(*handle->_db, Narrow(tableName), (size_t)rowIndex, Narrow(fieldName), Narrow(value));
            outcome->Success = r.success; outcome->Message = gcnew String(r.message.c_str());
            return outcome;
        }

        EditOutcome^ DuplicateRow(bool locale, String^ tableName, int rowIndex) {
            auto outcome = gcnew EditOutcome();
            NativeDatabaseHandle^ handle = locale ? _locale : _main;
            if (handle == nullptr) { outcome->Success = false; outcome->Message = "Database not loaded"; return outcome; }
            auto r = _engine->duplicateRow(*handle->_db, Narrow(tableName), (size_t)rowIndex);
            outcome->Success = r.success; outcome->Message = gcnew String(r.message.c_str());
            return outcome;
        }

        EditOutcome^ DeleteRow(bool locale, String^ tableName, int rowIndex) {
            auto outcome = gcnew EditOutcome();
            NativeDatabaseHandle^ handle = locale ? _locale : _main;
            if (handle == nullptr) { outcome->Success = false; outcome->Message = "Database not loaded"; return outcome; }
            auto r = _engine->deleteRow(*handle->_db, Narrow(tableName), (size_t)rowIndex);
            outcome->Success = r.success; outcome->Message = gcnew String(r.message.c_str());
            return outcome;
        }

        EditOutcome^ DeleteRowWithRelationships(bool locale, String^ tableName, int rowIndex) {
            auto outcome = gcnew EditOutcome();
            NativeDatabaseHandle^ handle = locale ? _locale : _main;
            if (handle == nullptr) { outcome->Success = false; outcome->Message = "Database not loaded"; return outcome; }
            auto r = _engine->deleteRowWithRelationships(*handle->_db, Narrow(tableName), (size_t)rowIndex);
            outcome->Success = r.success; outcome->Message = gcnew String(r.message.c_str());
            return outcome;
        }

        List<String^>^ ValidateIntegrity(bool locale) {
            auto issues = gcnew List<String^>();
            NativeDatabaseHandle^ handle = locale ? _locale : _main;
            if (handle == nullptr) { issues->Add("Database not loaded"); return issues; }
            for (const auto& issue : _engine->validateIntegrity(*handle->_db))
                issues->Add(gcnew String(issue.c_str()));
            return issues;
        }

        // Independently reload-verify a written file. Throws on failure. Read-only.
        void VerifyFile(String^ metaPath, String^ databasePath, bool encryptedLocale) {
            try {
                auto db = _engine->readT3db(Wide(metaPath), Wide(databasePath), encryptedLocale);
                if (db.tables.empty()) throw gcnew InvalidOperationException("Verification produced no tables");
            } catch (InvalidOperationException^) {
                throw;
            } catch (const std::exception& ex) {
                throw gcnew InvalidOperationException(
                    "Native database verification failed: " + gcnew String(ex.what()));
            } catch (...) {
                throw gcnew InvalidOperationException("Native database verification failed with an unknown engine error.");
            }
        }

        // Write a validated copy through the engine. Throws on failure. Never touches the source.
        void SaveCopy(bool locale, String^ outputPath) {
            NativeDatabaseHandle^ handle = locale ? _locale : _main;
            if (handle == nullptr) throw gcnew InvalidOperationException("Database not loaded");
            _engine->saveT3dbCopy(*handle->_db, Wide(outputPath));
        }

        // Raw bytes of a string cell (read-only), for codepage-specific decoding in managed code.
        array<System::Byte>^ GetCellBytes(bool locale, String^ tableName, int rowIndex, String^ fieldName) {
            NativeDatabaseHandle^ handle = locale ? _locale : _main;
            if (handle == nullptr) return gcnew array<System::Byte>(0);
            return handle->GetCellBytes(tableName, rowIndex, fieldName);
        }

        // Current in-memory value of a cell (reflects staged edits).
        String^ GetCellText(bool locale, String^ tableName, int rowIndex, String^ fieldName) {
            NativeDatabaseHandle^ handle = locale ? _locale : _main;
            if (handle == nullptr) return nullptr;
            int ti; auto t = FindTable(*handle->_db, Narrow(tableName), ti);
            if (!t || rowIndex < 0 || (size_t)rowIndex >= t->rows.size()) return nullptr;
            auto field = Narrow(fieldName);
            for (size_t c = 0; c < t->columns.size(); ++c)
                if (_stricmp(t->columns[c].name.c_str(), field.c_str()) == 0 || _stricmp(t->columns[c].shortName.c_str(), field.c_str()) == 0)
                    return CellText(t->rows[rowIndex].values[c]);
            return nullptr;
        }
    };

    List<TableInfo^>^ NativeDatabaseHandle::GetTables() {
        auto list = gcnew List<TableInfo^>();
        for (const auto& t : _db->tables) {
            auto info = gcnew TableInfo();
            info->Name = Managed(t.name); info->ShortName = Managed(t.shortName); info->RowCount = (int)t.rows.size();
            for (const auto& c : t.columns) {
                auto col = gcnew ColumnInfo();
                col->Name = Managed(c.name); col->ShortName = Managed(c.shortName);
                col->Kind = (FieldKind)(int)c.type; col->BitOffset = c.bitOffset; col->Depth = c.depth;
                col->RangeLow = c.rangeLow; col->RangeHigh = c.rangeHigh;
                col->IsWritable = c.type == cm26::NativeFieldType::Integer || c.type == cm26::NativeFieldType::Float
                    || c.type == cm26::NativeFieldType::String || c.type == cm26::NativeFieldType::ShortCompressedString
                    || c.type == cm26::NativeFieldType::LongCompressedString;
                info->Columns->Add(col);
            }
            list->Add(info);
        }
        return list;
    }

    int NativeDatabaseHandle::GetRowCount(String^ tableName) {
        int ti; auto t = detail::FindTable(*_db, detail::Narrow(tableName), ti);
        return t ? (int)t->rows.size() : 0;
    }

    RowData^ NativeDatabaseHandle::GetRow(String^ tableName, int rowIndex) {
        int ti; auto t = detail::FindTable(*_db, detail::Narrow(tableName), ti);
        if (!t || rowIndex < 0 || (size_t)rowIndex >= t->rows.size()) return nullptr;
        auto row = gcnew RowData(); row->Index = rowIndex;
        for (const auto& v : t->rows[rowIndex].values) row->Values->Add(detail::CellText(v));
        return row;
    }

    array<System::Byte>^ NativeDatabaseHandle::GetCellBytes(String^ tableName, int rowIndex, String^ fieldName) {
        int ti; auto t = detail::FindTable(*_db, detail::Narrow(tableName), ti);
        if (!t || rowIndex < 0 || (size_t)rowIndex >= t->rows.size()) return gcnew array<System::Byte>(0);
        auto field = detail::Narrow(fieldName);
        for (size_t c = 0; c < t->columns.size(); ++c)
            if (_stricmp(t->columns[c].name.c_str(), field.c_str()) == 0 || _stricmp(t->columns[c].shortName.c_str(), field.c_str()) == 0)
                return detail::CellBytes(t->rows[rowIndex].values[c]);
        return gcnew array<System::Byte>(0);
    }
}}
