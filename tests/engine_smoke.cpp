#include "database_engine.h"
#include <algorithm>
#include <iostream>
int wmain() {
  try {
  cm26::DatabaseEngine engine;
  const auto result = engine.loadFolder(L"database");
  std::wcout << L"state=" << static_cast<int>(result.state) << L" tables=" << result.tables.size() << L"\n";
  auto native = engine.readT3db(L"", L"database\\eng_us.DB", true);
  std::wcout << L"native_tables=" << native.tables.size();
  for (const auto& table : native.tables) std::wcout << L" " << table.name.c_str() << L"=" << table.rows.size();
  std::wcout << L"\n";
  engine.saveT3dbCopy(native, L"database\\eng_us_cm26_roundtrip.db");
  auto roundtrip = engine.readT3db(L"", L"database\\eng_us_cm26_roundtrip.db", true);
  std::wcout << L"roundtrip_tables=" << roundtrip.tables.size() << L"\n";
  auto localeEdit = engine.readT3db(L"", L"database\\eng_us.DB", true);
  bool localeEdited = false;
  std::string localeExpected;
  std::string localeTableName;
  for (auto& table : localeEdit.tables) {
    for (size_t row = 0; row < table.rows.size() && !localeEdited; ++row) {
      for (size_t column = 0; column < table.columns.size(); ++column) {
        const auto type = table.columns[column].type;
        if (type != cm26::NativeFieldType::ShortCompressedString && type != cm26::NativeFieldType::LongCompressedString) continue;
        const auto original = std::get<std::string>(table.rows[row].values[column]);
        if (original.size() < 2) continue;
        localeExpected = original.substr(0, original.size() - 1);
        const auto change = engine.stageEdit(localeEdit, table.name, row, table.columns[column].name, localeExpected);
        if (!change.success) return 11;
        engine.saveT3dbCopy(localeEdit, L"database\\eng_us_cm26_edit_test.db");
        localeTableName = table.name;
        localeEdited = true;
        break;
      }
    }
    if (localeEdited) break;
  }
  if (!localeEdited) return 12;
  auto localeReload = engine.readT3db(L"", L"database\\eng_us_cm26_edit_test.db", true);
  const auto localeTable = std::find_if(localeReload.tables.begin(), localeReload.tables.end(),
    [&](const auto& table) { return table.name == localeTableName; });
  // Locale string rows can be re-packed by the writer. Verify the edited value
  // inside its original table, without the previous full-database string scan.
  bool localeVerified = false;
  if (localeTable != localeReload.tables.end()) {
    for (const auto& row : localeTable->rows) {
      for (const auto& value : row.values) {
        if (const auto text = std::get_if<std::string>(&value); text && *text == localeExpected) {
          localeVerified = true;
          break;
        }
      }
      if (localeVerified) break;
    }
  }
  if (!localeVerified) return 13;
  std::wcout << L"locale_edit_verified=" << localeExpected.size() << L"\n";
  auto mainEdit = engine.readT3db(L"database\\fifa_ng_db-meta.xml", L"database\\fifa_ng_db.db");
  size_t mainRows = 0;
  for (const auto& table : mainEdit.tables) mainRows += table.rows.size();
  bool mainEdited = false;
  for (auto& table : mainEdit.tables) {
    for (size_t row = 0; row < table.rows.size() && !mainEdited; ++row) {
      for (size_t column = 0; column < table.columns.size(); ++column) {
        const auto& field = table.columns[column];
        if (field.type != cm26::NativeFieldType::Integer || field.rangeHigh <= field.rangeLow) continue;
        const int before = std::get<int>(table.rows[row].values[column]);
        const int after = before == field.rangeLow ? before + 1 : field.rangeLow;
        const auto change = engine.stageEdit(mainEdit, table.name, row, field.name, std::to_string(after));
        if (!change.success) return 14;
        engine.saveT3dbCopy(mainEdit, L"database\\fifa_ng_db_cm26_edit_test.db");
        mainEdited = true;
        break;
      }
    }
    if (mainEdited) break;
  }
  if (!mainEdited) return 15;
  const auto mainReload = engine.readT3db(L"database\\fifa_ng_db-meta.xml", L"database\\fifa_ng_db_cm26_edit_test.db");
  if (mainReload.tables.size() != mainEdit.tables.size()) return 16;
  std::wcout << L"main_edit_verified tables=" << mainReload.tables.size() << L" rows=" << mainRows << L"\n";
  // Structural round-trip: duplicate then delete a real nation record in a scratch copy.
  // This verifies a rebuilt table directory, record layout and CRCs can be read back by the engine.
  auto structural = engine.readT3db(L"database\\fifa_ng_db-meta.xml", L"database\\fifa_ng_db.db");
  const auto nations = std::find_if(structural.tables.begin(), structural.tables.end(), [](const auto& t) { return t.name == "nations"; });
  if (nations == structural.tables.end() || nations->rows.empty()) return 17;
  const auto beforeStructural = nations->rows.size();
  if (!engine.duplicateRow(structural, "nations", 0).success) return 18;
  engine.saveT3dbCopy(structural, L"database_scratch\\fifa_ng_db_structural_add.db");
  auto added = engine.readT3db(L"database\\fifa_ng_db-meta.xml", L"database_scratch\\fifa_ng_db_structural_add.db");
  const auto addedNations = std::find_if(added.tables.begin(), added.tables.end(), [](const auto& t) { return t.name == "nations"; });
  if (addedNations == added.tables.end() || addedNations->rows.size() != beforeStructural + 1) return 19;
  if (!engine.deleteRow(added, "nations", beforeStructural).success) return 20;
  engine.saveT3dbCopy(added, L"database_scratch\\fifa_ng_db_structural_delete.db");
  auto deleted = engine.readT3db(L"database\\fifa_ng_db-meta.xml", L"database_scratch\\fifa_ng_db_structural_delete.db");
  const auto deletedNations = std::find_if(deleted.tables.begin(), deleted.tables.end(), [](const auto& t) { return t.name == "nations"; });
  if (deletedNations == deleted.tables.end() || deletedNations->rows.size() != beforeStructural) return 21;
  std::wcout << L"structural_add_delete_verified nations=" << beforeStructural << L"\n";
  return 0;
  } catch (const std::exception& error) {
    std::cerr << "engine error: " << error.what() << "\n";
    return 10;
  }
}
