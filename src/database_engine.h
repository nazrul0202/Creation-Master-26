#pragma once
#include <filesystem>
#include <string>
#include <variant>
#include <vector>

namespace cm26 {
enum class LoadState { Success, MissingFile, WrongKeyOrIv, UnsupportedOrCorrupt };
struct Field { std::wstring name, shortName, type, foreignTable; int rangeLow{}, rangeHigh{}; unsigned depth{}; bool key{}; };
struct Table { std::wstring name, shortName; std::vector<Field> fields; };
struct LoadResult { LoadState state; std::wstring message; std::vector<Table> tables; std::filesystem::path meta, database, localization; };

// Native T3DB v8 in-memory model. Values are decoded from actual database rows.
enum class NativeFieldType : int { String = 0, Integer = 3, Float = 4, ShortCompressedString = 13, LongCompressedString = 14 };
using CellValue = std::variant<int, float, std::string>;
struct NativeColumn { NativeFieldType type{}; std::string name, shortName, foreignTable; int bitOffset{}, depth{}, rangeLow{}, rangeHigh{}; bool key{}; };
struct NativeRow { std::vector<CellValue> values, originalValues; std::vector<unsigned char> originalBytes; size_t sourceRecordIndex{}; };
struct NativeTable { std::string name, shortName; unsigned flags{}, recordSize{}, recordCount{}, validRecordCount{}, compressedBytes{}; std::vector<NativeColumn> columns; std::vector<NativeRow> rows; size_t tableOffset{}, tableEndOffset{}, recordDataOffset{}, tableCrcOffset{}, recordsCrcOffset{}; bool structuralChanged{}; };
struct NativeDatabase { bool littleEndian{true}, encrypted{}; size_t headerCrcOffset{}, shortNamesCrcOffset{}; std::vector<unsigned char> bytes; std::vector<NativeTable> tables; };
struct EditResult { bool success{}; std::string message; };

class DatabaseEngine {
public:
  // Loads only after validating the required FC database set. It never changes source files.
  LoadResult loadFolder(const std::filesystem::path& folder) const;
  // Reads every native row. Does not mutate the source file.
  NativeDatabase readT3db(const std::filesystem::path& metaPath, const std::filesystem::path& dbPath, bool encryptedLocale = false) const;
  // Stages a validated edit in memory. No bytes are written until saveT3dbCopy is called.
  EditResult stageEdit(NativeDatabase& database, const std::string& tableName, size_t rowIndex, const std::string& fieldName, const std::string& textValue) const;
  EditResult duplicateRow(NativeDatabase& database, const std::string& tableName, size_t rowIndex) const;
  EditResult deleteRow(NativeDatabase& database, const std::string& tableName, size_t rowIndex) const;
  // CM16-style entity deletion: removes dependent link rows and clears optional references before deleting the parent.
  EditResult deleteRowWithRelationships(NativeDatabase& database, const std::string& tableName, size_t rowIndex) const;
  // Verifies key uniqueness and foreign-key targets before any file is written.
  std::vector<std::string> validateIntegrity(const NativeDatabase& database) const;
  // Writes a new valid file. Integer, float, fixed strings and in-place compressed locale strings are supported.
  // Compressed text must fit the allocation already present in its database table.
  void saveT3dbCopy(const NativeDatabase& database, const std::filesystem::path& outputPath) const;
private:
  static bool isT3db(const std::vector<unsigned char>& bytes);
  static bool decryptEngUs(const std::vector<unsigned char>& encrypted, std::vector<unsigned char>& plain);
  static bool encryptEngUs(const std::vector<unsigned char>& plain, std::vector<unsigned char>& encrypted);
  static std::vector<Table> parseMetaXml(const std::wstring& xml);
};
}
