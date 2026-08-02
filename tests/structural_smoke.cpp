#include "database_engine.h"
#include <algorithm>
#include <iostream>

int wmain() {
  try {
    cm26::DatabaseEngine engine;
    auto database = engine.readT3db(L"database\\fifa_ng_db-meta.xml", L"database\\fifa_ng_db.db");
    const auto before = std::find_if(database.tables.begin(), database.tables.end(), [](const auto& t) { return t.name == "nations"; });
    if (before == database.tables.end() || before->rows.empty()) return 2;
    const auto count = before->rows.size();
    const auto add = engine.duplicateRow(database, "nations", 0);
    if (!add.success) { std::cerr << add.message << "\n"; return 3; }
    engine.saveT3dbCopy(database, L"database_scratch\\fifa_ng_db_structural_add.db");
    auto added = engine.readT3db(L"database\\fifa_ng_db-meta.xml", L"database_scratch\\fifa_ng_db_structural_add.db");
    const auto nation = std::find_if(added.tables.begin(), added.tables.end(), [](const auto& t) { return t.name == "nations"; });
    if (nation == added.tables.end() || nation->rows.size() != count + 1) return 4;
    std::cout << "structural add verified; nations=" << count << " -> " << nation->rows.size() << "\n";
    return 0;
  } catch (const std::exception& error) { std::cerr << error.what() << "\n"; return 1; }
}
