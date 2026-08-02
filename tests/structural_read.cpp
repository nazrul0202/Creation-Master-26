#include "database_engine.h"
#include <algorithm>
#include <iostream>

int wmain() {
  try {
    cm26::DatabaseEngine engine;
    const auto database = engine.readT3db(L"database\\fifa_ng_db-meta.xml", L"database\\fifa_ng_db.db");
    const auto nation = std::find_if(database.tables.begin(), database.tables.end(), [](const auto& t) { return t.name == "nations"; });
    if (nation == database.tables.end() || nation->rows.size() != 218) return 2;
    const auto issues = engine.validateIntegrity(database);
    std::cout << "integrity verified; tables=" << database.tables.size() << " nations=" << nation->rows.size() << " issues=" << issues.size() << "\n";
    for (size_t i = 0; i < issues.size() && i < 5; ++i) std::cout << issues[i] << "\n";
    if (!issues.empty()) return 3;
    auto duplicateProbe = database;
    if (!engine.duplicateRow(duplicateProbe, "nations", 0).success) return 4;
    const auto duplicateIssues = engine.validateIntegrity(duplicateProbe);
    if (duplicateIssues.empty()) return 5;
    std::cout << "duplicate-key guard verified; issues=" << duplicateIssues.size() << "\n";
    auto releaseProbe = database;
    if (!engine.deleteRow(releaseProbe, "teamplayerlinks", 0).success) return 6;
    const auto releaseIssues = engine.validateIntegrity(releaseProbe);
    if (!releaseIssues.empty()) return 7;
    std::cout << "teamplayerlinks release guard verified; issues=0\n";
    return 0;
  } catch (const std::exception& error) { std::cerr << error.what() << "\n"; return 1; }
}
