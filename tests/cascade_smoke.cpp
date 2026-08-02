#include "database_engine.h"
#include <iostream>

int wmain() {
  try {
    cm26::DatabaseEngine engine;
    auto db = engine.readT3db(L"database\\fifa_ng_db-meta.xml", L"database\\fifa_ng_db.db");
    auto findTable = [&](const std::string& name) -> cm26::NativeTable* { for (auto& t : db.tables) if (t.name == name) return &t; return nullptr; };
    auto* players = findTable("players"); auto* links = findTable("teamplayerlinks");
    if (!players || !links || links->rows.empty()) return 2;
    size_t playerIdColumn = 0; for (; playerIdColumn < links->columns.size(); ++playerIdColumn) if (links->columns[playerIdColumn].name == "playerid") break;
    if (playerIdColumn == links->columns.size()) return 3;
    const int playerId = std::get<int>(links->rows[0].values[playerIdColumn]);
    size_t playerKey = 0; for (; playerKey < players->columns.size(); ++playerKey) if (players->columns[playerKey].name == "playerid") break;
    size_t playerRow = 0; for (; playerRow < players->rows.size(); ++playerRow) if (std::get<int>(players->rows[playerRow].values[playerKey]) == playerId) break;
    if (playerRow == players->rows.size()) return 4;
    const auto linksBefore = links->rows.size();
    const auto deleted = engine.deleteRowWithRelationships(db, "players", playerRow);
    if (!deleted.success || players->rows.size() != 20267 || links->rows.size() >= linksBefore) { std::cerr << deleted.message << "\n"; return 5; }
    // Prove the deleted player key is no longer referenced by any FK to players.
    for (const auto& table : db.tables) for (size_t c = 0; c < table.columns.size(); ++c) {
      if (table.columns[c].foreignTable != "players") continue;
      for (const auto& row : table.rows) {
        if (const auto* value = std::get_if<int>(&row.values[c]); value && *value == playerId) {
          std::cerr << "dangling player FK in " << table.name << "." << table.columns[c].name << "\n";
          return 6;
        }
      }
    }
    const auto issues = engine.validateIntegrity(db);
    if (!issues.empty()) { std::cerr << issues.front() << "\n"; return 7; }
    engine.saveT3dbCopy(db, L"database_scratch\\fifa_ng_db_cascade_player_delete.db");
    auto reloaded = engine.readT3db(L"database\\fifa_ng_db-meta.xml", L"database_scratch\\fifa_ng_db_cascade_player_delete.db");
    auto* reloadedPlayers = findTable("not-used");
    for (auto& table : reloaded.tables) if (table.name == "players") { reloadedPlayers = &table; break; }
    if (!reloadedPlayers || reloadedPlayers->rows.size() != players->rows.size()) return 8;
    std::cout << deleted.message << "\n";
    return 0;
  } catch (const std::exception& ex) { std::cerr << ex.what() << "\n"; return 1; }
}
