#include "database_engine.h"
#include <windows.h>
#include <bcrypt.h>
#include <algorithm>
#include <array>
#include <charconv>
#include <fstream>
#include <regex>
#include <sstream>
#include <bit>
#include <cstring>
#include <cctype>
#include <cstdlib>
#include <climits>
#include <stdexcept>
#include <unordered_map>
#include <set>

namespace cm26 {
namespace {
std::vector<unsigned char> readBytes(const std::filesystem::path& p) {
  std::ifstream f(p, std::ios::binary | std::ios::ate);
  if (!f) return {};
  const auto n = f.tellg(); if (n <= 0) return {};
  std::vector<unsigned char> b(static_cast<size_t>(n)); f.seekg(0); f.read(reinterpret_cast<char*>(b.data()), n); return b;
}
std::wstring readText(const std::filesystem::path& p) {
  std::wifstream f(p); f.imbue(std::locale("")); std::wstringstream s; s << f.rdbuf(); return s.str();
}
std::filesystem::path findInsensitive(const std::filesystem::path& dir, std::wstring wanted) {
  std::transform(wanted.begin(), wanted.end(), wanted.begin(), towlower);
  for (const auto& e : std::filesystem::directory_iterator(dir)) { auto n=e.path().filename().wstring(); std::transform(n.begin(),n.end(),n.begin(),towlower); if(n==wanted) return e.path(); }
  return {};
}
std::vector<Table> builtInLocaleSchema() {
  const auto makeTable = [](const wchar_t* name, const wchar_t* shortName) {
    Table table{};
    table.name = name; table.shortName = shortName;
    table.fields = {
      Field{L"hashid", L"jhKj", L"FIELDTYPE_INTEGER", L"", -2147483647 - 1, 2147483647, 32, true},
      Field{L"stringid", L"sAhV", L"FIELDTYPE_STRING", L"", 0, 0, 800, false},
      Field{L"sourcetext", L"ZbYb", L"FIELDTYPE_STRING", L"", 0, 0, 64000, false}
    };
    return table;
  };
  return { makeTable(L"LanguageStrings1", L"gFXC"), makeTable(L"LanguageStrings2", L"dMSI") };
}
std::wstring attr(const std::wstring& tag, const std::wstring& name) {
  const auto key = L" " + name + L"=\"";
  const auto start = tag.find(key);
  if (start == std::wstring::npos) return L"";
  const auto value = start + key.size(), end = tag.find(L'\"', value);
  return end == std::wstring::npos ? L"" : tag.substr(value, end - value);
}
uint16_t u16(const std::vector<unsigned char>& b,size_t p,bool le){if(p+2>b.size())throw std::runtime_error("T3DB truncated");return le?uint16_t(b[p]|b[p+1]<<8):uint16_t(b[p+1]|b[p]<<8);}
uint32_t u32(const std::vector<unsigned char>& b,size_t p,bool le){if(p+4>b.size())throw std::runtime_error("T3DB truncated");return le?uint32_t(b[p])|uint32_t(b[p+1])<<8|uint32_t(b[p+2])<<16|uint32_t(b[p+3])<<24:uint32_t(b[p+3])|uint32_t(b[p+2])<<8|uint32_t(b[p+1])<<16|uint32_t(b[p])<<24;}
void put16(std::vector<unsigned char>& b,size_t p,uint16_t v,bool le){if(p+2>b.size())throw std::runtime_error("T3DB write out of bounds");for(int i=0;i<2;i++)b[p+(le?i:1-i)]=static_cast<unsigned char>(v>>(8*i));}
void put32(std::vector<unsigned char>& b,size_t p,uint32_t v,bool le){if(p+4>b.size())throw std::runtime_error("T3DB write out of bounds");for(int i=0;i<4;i++)b[p+(le?i:3-i)]=static_cast<unsigned char>(v>>(8*i));}
std::string n4(const std::vector<unsigned char>& b,size_t p){if(p+4>b.size())throw std::runtime_error("T3DB truncated shortname");size_t n=0;while(n<4&&b[p+n])++n;return std::string(reinterpret_cast<const char*>(b.data()+p),n);}
int bits(const std::vector<unsigned char>& r,int off,int len,bool le){uint64_t x=0;if(le){for(int i=0;i<(off%8+len+7)/8&&off/8+i<int(r.size());++i)x|=uint64_t(r[off/8+i])<<(8*i);return int((x>>(off%8))&((len>=32)?0xffffffffULL:((1ULL<<len)-1)));}for(int i=0;i<len;i++)x=(x<<1)|((r[(off+i)/8]>>(7-((off+i)%8)))&1);return int(x);}
void putBits(std::vector<unsigned char>& r,int off,int len,uint32_t v,bool le){if(le){for(int i=0;i<len;i++){auto& q=r[(off+i)/8];unsigned char m=1u<<((off+i)%8);q=(q&~m)|(((v>>i)&1)?m:0);}}else for(int i=0;i<len;i++){auto& q=r[(off+i)/8];unsigned char m=1u<<(7-((off+i)%8));q=(q&~m)|(((v>>(len-1-i))&1)?m:0);}}
uint32_t crcDb11(const std::vector<unsigned char>& b,size_t p,size_t n){uint32_t c=0xffffffffU;for(size_t i=0;i<n;i++){c^=uint32_t(b[p+i])<<24;for(int x=0;x<8;x++)c=(c&0x80000000U)?(c<<1)^0x04C11DB7U:c<<1;}return c;}
struct Huff { std::vector<std::array<unsigned char,2>> child,leaf; explicit Huff(size_t n):child(n),leaf(n){} std::string read(const std::vector<unsigned char>& d,size_t p,size_t out)const{std::string s;s.reserve(out);if(child.empty()){if(p+out>d.size())throw std::runtime_error("compressed string out of bounds");return std::string(reinterpret_cast<const char*>(d.data()+p),out);}size_t node=0;while(s.size()<out){if(p>=d.size())throw std::runtime_error("compressed string truncated");auto x=d[p++];for(int i=7;i>=0&&s.size()<out;i++){int dir=(x>>i)&1;auto c=child[node][dir];if(!c){s.push_back(char(leaf[node][dir]));node=0;}else {if(c>=child.size())throw std::runtime_error("invalid Huffman tree");node=c;}}}return s;}};

using HuffCode = std::vector<bool>;

void collectHuffCodes(const Huff& tree, size_t node, HuffCode& path, std::array<HuffCode, 256>& codes, std::array<bool, 256>& present, unsigned depth = 0) {
  if (node >= tree.child.size() || depth > 512) throw std::runtime_error("Invalid Huffman tree");
  for (int bit = 0; bit < 2; ++bit) {
    path.push_back(bit != 0);
    const unsigned char child = tree.child[node][bit];
    if (child == 0) {
      const auto symbol = tree.leaf[node][bit];
      if (present[symbol] && codes[symbol] != path) throw std::runtime_error("Ambiguous Huffman symbol");
      codes[symbol] = path;
      present[symbol] = true;
    } else {
      collectHuffCodes(tree, child, path, codes, present, depth + 1);
    }
    path.pop_back();
  }
}

std::vector<unsigned char> encodeHuff(const Huff& tree, const std::string& text) {
  if (tree.child.empty()) return std::vector<unsigned char>(text.begin(), text.end());
  std::array<HuffCode, 256> codes;
  std::array<bool, 256> present{};
  HuffCode path;
  collectHuffCodes(tree, 0, path, codes, present);
  std::vector<unsigned char> output;
  unsigned char current = 0;
  int used = 0;
  for (const unsigned char symbol : text) {
    if (!present[symbol]) throw std::runtime_error("Text contains a character not present in this locale Huffman tree");
    for (const bool bit : codes[symbol]) {
      current = static_cast<unsigned char>((current << 1) | (bit ? 1 : 0));
      if (++used == 8) { output.push_back(current); current = 0; used = 0; }
    }
  }
  if (used) output.push_back(static_cast<unsigned char>(current << (8 - used)));
  return output;
}

void rewriteCompressedStrings(std::vector<unsigned char>& bytes, const NativeDatabase& database, const NativeTable& table) {
  struct Change { std::string text; bool shortString{}; };
  std::set<size_t> offsets;
  std::unordered_map<size_t, Change> changes;
  bool hasCompressed = false;
  for (const auto& row : table.rows) {
    for (size_t column = 0; column < table.columns.size(); ++column) {
      const auto& field = table.columns[column];
      const bool compressed = field.type == NativeFieldType::ShortCompressedString || field.type == NativeFieldType::LongCompressedString;
      if (!compressed) continue;
      hasCompressed = true;
      const size_t byte = static_cast<size_t>(field.bitOffset / 8);
      if (byte + 4 > row.originalBytes.size()) throw std::runtime_error("Compressed string descriptor exceeds record boundary");
      const int offset = static_cast<int>(u32(row.originalBytes, byte, database.littleEndian));
      if (offset >= 0) offsets.insert(static_cast<size_t>(offset));
      if (row.values[column] == row.originalValues[column]) continue;
      if (offset < 0) throw std::runtime_error("Cannot add a new compressed string to an empty locale slot");
      const Change change{std::get<std::string>(row.values[column]), field.type == NativeFieldType::ShortCompressedString};
      const auto [it, inserted] = changes.emplace(static_cast<size_t>(offset), change);
      if (!inserted && (it->second.text != change.text || it->second.shortString != change.shortString)) {
        throw std::runtime_error("Shared locale string has conflicting edits");
      }
    }
  }
  if (!hasCompressed || changes.empty()) return;
  if (offsets.empty()) throw std::runtime_error("Compressed locale table has no string offsets");

  const size_t blob = table.recordDataOffset + static_cast<size_t>(table.recordSize) * table.recordCount;
  const size_t treeSize = *offsets.begin();
  if (blob > bytes.size() || treeSize > table.compressedBytes || blob + table.compressedBytes > bytes.size()) {
    throw std::runtime_error("Compressed locale blob exceeds database boundary");
  }
  if (treeSize % 4 != 0) throw std::runtime_error("Invalid locale Huffman tree size");
  Huff tree(treeSize / 4);
  for (size_t i = 0; i < tree.child.size(); ++i) {
    tree.child[i][0] = bytes[blob + i * 4]; tree.leaf[i][0] = bytes[blob + i * 4 + 1];
    tree.child[i][1] = bytes[blob + i * 4 + 2]; tree.leaf[i][1] = bytes[blob + i * 4 + 3];
  }
  std::vector<size_t> ordered(offsets.begin(), offsets.end());
  for (const auto& [offset, change] : changes) {
    const auto next = std::upper_bound(ordered.begin(), ordered.end(), offset);
    const size_t end = next == ordered.end() ? table.compressedBytes : *next;
    if (offset >= end || end > table.compressedBytes) throw std::runtime_error("Invalid compressed string allocation");
    const auto payload = encodeHuff(tree, change.text);
    const size_t header = change.shortString ? 1 : 2;
    const size_t lengthLimit = change.shortString ? 255 : 65535;
    if (change.text.size() > lengthLimit) throw std::runtime_error("Locale text exceeds the field length limit");
    if (header + payload.size() > end - offset) throw std::runtime_error("Edited locale text does not fit its existing compressed allocation");
    const size_t position = blob + offset;
    std::fill(bytes.begin() + position, bytes.begin() + blob + end, static_cast<unsigned char>(0));
    if (change.shortString) bytes[position] = static_cast<unsigned char>(change.text.size());
    else { bytes[position] = static_cast<unsigned char>(change.text.size() >> 8); bytes[position + 1] = static_cast<unsigned char>(change.text.size()); }
    std::copy(payload.begin(), payload.end(), bytes.begin() + position + header);
  }
}
}

bool DatabaseEngine::isT3db(const std::vector<unsigned char>& b) { return b.size() >= 8 && b[0]=='D' && b[1]=='B' && b[2]==0 && b[3]==8; }

bool DatabaseEngine::decryptEngUs(const std::vector<unsigned char>& encrypted, std::vector<unsigned char>& plain) {
  // AES-256-CBC, no padding: the supplied locale file is encrypted block-for-block.
  static const UCHAR key[32] = { 0x8F,0x5B,0xCA,0x17,0x7B,0x44,0x2B,0x80,0x2C,0x8C,0xCC,0xAA,0xB4,0x12,0x7E,0x69,0x54,0x5A,0xC0,0xCC,0x8B,0x9E,0x18,0xB9,0x29,0x8A,0x48,0x13,0x9F,0x31,0xEF,0x5F };
  static const UCHAR iv[16] = { 0x7A,0xDC,0xDF,0x10,0x90,0x12,0x1E,0xD1,0x97,0xC3,0xA9,0x88,0x51,0xAA,0x61,0x6E };
  if (encrypted.empty() || encrypted.size()%16) return false;
  BCRYPT_ALG_HANDLE alg{}; BCRYPT_KEY_HANDLE hkey{}; DWORD objSize{},cb{}, out{};
  if (BCryptOpenAlgorithmProvider(&alg,BCRYPT_AES_ALGORITHM,nullptr,0) || BCryptSetProperty(alg,BCRYPT_CHAINING_MODE,reinterpret_cast<PUCHAR>(const_cast<wchar_t*>(BCRYPT_CHAIN_MODE_CBC)),sizeof(BCRYPT_CHAIN_MODE_CBC),0) || BCryptGetProperty(alg,BCRYPT_OBJECT_LENGTH,reinterpret_cast<PUCHAR>(&objSize),sizeof(objSize),&cb,0)) return false;
  std::vector<UCHAR> obj(objSize), mutableIv(std::begin(iv),std::end(iv));
  NTSTATUS status=BCryptGenerateSymmetricKey(alg,&hkey,obj.data(),objSize,const_cast<PUCHAR>(key),sizeof(key),0);
  plain.resize(encrypted.size());
  if (!status) status=BCryptDecrypt(hkey,const_cast<PUCHAR>(encrypted.data()),static_cast<ULONG>(encrypted.size()),nullptr,mutableIv.data(),static_cast<ULONG>(mutableIv.size()),plain.data(),static_cast<ULONG>(plain.size()),&out,0);
  if(hkey) BCryptDestroyKey(hkey); BCryptCloseAlgorithmProvider(alg,0); plain.resize(out); return !status;
}

bool DatabaseEngine::encryptEngUs(const std::vector<unsigned char>& plain, std::vector<unsigned char>& encrypted) {
  static const UCHAR key[32] = { 0x8F,0x5B,0xCA,0x17,0x7B,0x44,0x2B,0x80,0x2C,0x8C,0xCC,0xAA,0xB4,0x12,0x7E,0x69,0x54,0x5A,0xC0,0xCC,0x8B,0x9E,0x18,0xB9,0x29,0x8A,0x48,0x13,0x9F,0x31,0xEF,0x5F };
  static const UCHAR iv[16] = { 0x7A,0xDC,0xDF,0x10,0x90,0x12,0x1E,0xD1,0x97,0xC3,0xA9,0x88,0x51,0xAA,0x61,0x6E };
  if (plain.empty() || plain.size()%16) return false;
  BCRYPT_ALG_HANDLE alg{}; BCRYPT_KEY_HANDLE hkey{}; DWORD objSize{},cb{},out{};
  if (BCryptOpenAlgorithmProvider(&alg,BCRYPT_AES_ALGORITHM,nullptr,0) || BCryptSetProperty(alg,BCRYPT_CHAINING_MODE,reinterpret_cast<PUCHAR>(const_cast<wchar_t*>(BCRYPT_CHAIN_MODE_CBC)),sizeof(BCRYPT_CHAIN_MODE_CBC),0) || BCryptGetProperty(alg,BCRYPT_OBJECT_LENGTH,reinterpret_cast<PUCHAR>(&objSize),sizeof(objSize),&cb,0)) return false;
  std::vector<UCHAR> obj(objSize), mutableIv(std::begin(iv),std::end(iv)); encrypted.resize(plain.size());
  NTSTATUS status=BCryptGenerateSymmetricKey(alg,&hkey,obj.data(),objSize,const_cast<PUCHAR>(key),sizeof(key),0);
  if(!status) status=BCryptEncrypt(hkey,const_cast<PUCHAR>(plain.data()),static_cast<ULONG>(plain.size()),nullptr,mutableIv.data(),static_cast<ULONG>(mutableIv.size()),encrypted.data(),static_cast<ULONG>(encrypted.size()),&out,0);
  if(hkey) BCryptDestroyKey(hkey); BCryptCloseAlgorithmProvider(alg,0); encrypted.resize(out); return !status;
}

std::vector<Table> DatabaseEngine::parseMetaXml(const std::wstring& xml) {
  std::vector<Table> out;
  for (size_t pos = 0; (pos = xml.find(L"<table ", pos)) != std::wstring::npos;) {
    const auto openEnd = xml.find(L'>', pos), close = xml.find(L"</table>", openEnd);
    if (openEnd == std::wstring::npos || close == std::wstring::npos) break;
    const auto opener = xml.substr(pos, openEnd - pos + 1); Table t{attr(opener,L"name"),attr(opener,L"shortname")};
    const auto body = xml.substr(openEnd + 1, close - openEnd - 1);
    for (size_t fieldPos = 0; (fieldPos = body.find(L"<field ", fieldPos)) != std::wstring::npos;) { const auto end = body.find(L"/>", fieldPos); if(end == std::wstring::npos) break; auto tag=body.substr(fieldPos,end-fieldPos+2); Field f{attr(tag,L"name"),attr(tag,L"shortname"),attr(tag,L"type"),attr(tag,L"fkparenttable")}; try { f.depth=std::stoul(attr(tag,L"depth"));f.rangeLow=std::stoi(attr(tag,L"rangelow"));f.rangeHigh=std::stoi(attr(tag,L"rangehigh")); } catch(...) {} f.key=attr(tag,L"key")==L"True"; t.fields.push_back(std::move(f)); fieldPos=end+2; }
    out.push_back(std::move(t)); pos = close + 8;
  }
  return out;
}

LoadResult DatabaseEngine::loadFolder(const std::filesystem::path& folder) const {
  LoadResult r{}; if (!std::filesystem::is_directory(folder)) {r.state=LoadState::MissingFile;r.message=L"Folder database tidak sah.";return r;}
  r.meta=findInsensitive(folder,L"fifa_ng_db-meta.xml");
  r.database=findInsensitive(folder,L"fifa_ng_db.db"); r.localization=findInsensitive(folder,L"eng_us.db");
  if(r.meta.empty()||r.database.empty()||r.localization.empty()) { r.state=LoadState::MissingFile; r.message=L"Set tidak lengkap: perlukan metadata, fifa_ng_db.db dan eng_us.db."; return r; }
  const auto mainDb=readBytes(r.database); if(!isT3db(mainDb)){r.state=LoadState::UnsupportedOrCorrupt;r.message=L"fifa_ng_db.db bukan T3DB yang sah atau rosak.";return r;}
  std::vector<unsigned char> loc; if(!decryptEngUs(readBytes(r.localization),loc)||!isT3db(loc)){r.state=LoadState::WrongKeyOrIv;r.message=L"eng_us.db gagal didekripsi: kunci/IV tidak sepadan atau fail rosak.";return r;}
  r.tables=parseMetaXml(readText(r.meta)); if(r.tables.empty()){r.state=LoadState::UnsupportedOrCorrupt;r.message=L"Metadata XML tidak mengandungi struktur jadual yang boleh dibaca.";return r;}
  r.state=LoadState::Success;r.message=L"Load successful – fail sah. Tiada fail sumber telah diubah.";return r;
}

NativeDatabase DatabaseEngine::readT3db(const std::filesystem::path& metaPath,const std::filesystem::path& dbPath,bool encryptedLocale) const {
  auto raw=readBytes(dbPath); if(raw.empty()) throw std::runtime_error("Cannot open database");
  NativeDatabase db; db.encrypted=encryptedLocale; if(encryptedLocale&&!decryptEngUs(raw,db.bytes)) throw std::runtime_error("Locale decryption failed"); if(!encryptedLocale) db.bytes=std::move(raw);
  auto& b=db.bytes; if(!isT3db(b))throw std::runtime_error("Not a T3DB v8 file"); bool le=b[4]!=1;db.littleEndian=le; if(unsigned(b[2])*255u+b[3]!=8)throw std::runtime_error("Unsupported T3DB version");
  auto schema=parseMetaXml(readText(metaPath)); if(schema.empty() && encryptedLocale) schema=builtInLocaleSchema(); if(schema.empty()) throw std::runtime_error("Metadata XML is missing or invalid"); std::unordered_map<std::string,Field> fields;std::unordered_map<std::string,std::string> tables;
  auto narrow=[](const std::wstring&s){std::string r;r.reserve(s.size());for(wchar_t ch:s)r.push_back(static_cast<char>(ch));return r;};for(const auto&t:schema){tables[narrow(t.shortName)]=narrow(t.name);for(const auto&f:t.fields)fields[narrow(t.shortName)+"/"+narrow(f.shortName)]=f;}
  uint32_t fileSize=u32(b,8,le),count=u32(b,16,le);if(count>10000)throw std::runtime_error("Invalid table count"); db.headerCrcOffset=20;size_t list=24; if(list+size_t(count)*8+4>b.size())throw std::runtime_error("Invalid table directory");db.shortNamesCrcOffset=list+size_t(count)*8;
  std::vector<uint32_t> offsets(count);std::vector<std::string> shorts(count);for(uint32_t i=0;i<count;i++){shorts[i]=n4(b,list+i*8);offsets[i]=u32(b,list+i*8+4,le);} size_t base=db.shortNamesCrcOffset+4, end=fileSize<b.size()?fileSize:b.size();
  for(uint32_t ti=0;ti<count;ti++){size_t p=base+offsets[ti],limit=ti+1<count?base+offsets[ti+1]:end;if(p+36>limit)throw std::runtime_error("Invalid table header");NativeTable t;t.tableOffset=p;t.tableEndOffset=limit;t.shortName=shorts[ti];t.name=tables.contains(t.shortName)?tables[t.shortName]:t.shortName;t.flags=u32(b,p,le);t.recordSize=u32(b,p+4,le);t.compressedBytes=u32(b,p+12,le);t.recordCount=u16(b,p+16,le);t.validRecordCount=u16(b,p+18,le);unsigned cols=b[p+24],indexes=b[p+25];t.tableCrcOffset=p+32;size_t q=p+36;if(q+size_t(cols)*16>limit||t.recordSize>1<<20)throw std::runtime_error("Invalid table descriptor");
    for(unsigned ci=0;ci<cols;ci++){NativeColumn c;c.type=NativeFieldType(static_cast<int>(u32(b,q,le)));c.bitOffset=int(u32(b,q+4,le));c.shortName=n4(b,q+8);c.depth=int(u32(b,q+12,le));auto it=fields.find(t.shortName+"/"+c.shortName);if(it==fields.end()){c.name=c.shortName;c.rangeLow=0;c.rangeHigh=c.depth>=31?INT_MAX:static_cast<int>((1ULL<<c.depth)-1);}else{c.name=narrow(it->second.name);c.rangeLow=it->second.rangeLow;c.rangeHigh=it->second.rangeHigh;}
      if(it!=fields.end()){c.key=it->second.key;c.foreignTable=narrow(it->second.foreignTable);}t.columns.push_back(std::move(c));q+=16;}
    t.recordDataOffset=q;size_t recordsBytes=size_t(t.recordSize)*t.recordCount;if(q+recordsBytes>limit)throw std::runtime_error("Record area exceeds table");int minOff=INT_MAX;std::vector<std::pair<size_t,int>> compressed;
    for(unsigned ri=0;ri<t.validRecordCount;ri++){NativeRow r;r.sourceRecordIndex=ri;r.originalBytes.assign(b.begin()+q+size_t(ri)*t.recordSize,b.begin()+q+size_t(ri+1)*t.recordSize);for(size_t ci=0;ci<t.columns.size();ci++){auto&c=t.columns[ci];if(c.type==NativeFieldType::Integer)r.values.emplace_back(bits(r.originalBytes,c.bitOffset,c.depth,le)+c.rangeLow);else if(c.type==NativeFieldType::Float){size_t o=c.bitOffset/8;if(o+4>r.originalBytes.size())throw std::runtime_error("Float out of bounds");uint32_t v=u32(r.originalBytes,o,le);r.values.emplace_back(std::bit_cast<float>(v));}else if(c.type==NativeFieldType::String){size_t o=c.bitOffset/8,n=c.depth/8;if(o+n>r.originalBytes.size())throw std::runtime_error("String out of bounds");while(n&&r.originalBytes[o+n-1]==0)--n;r.values.emplace_back(std::string(reinterpret_cast<const char*>(r.originalBytes.data()+o),n));}else if(c.type==NativeFieldType::ShortCompressedString||c.type==NativeFieldType::LongCompressedString){int off=int(u32(r.originalBytes,c.bitOffset/8,le));r.values.emplace_back(off);if(off>=0&&off<minOff)minOff=off;compressed.push_back({r.values.size()-1,off});}else throw std::runtime_error("Unknown T3DB column type");}t.rows.push_back(std::move(r));}
    size_t blob=q+recordsBytes,indexStart=blob+((t.compressedBytes+7)&~size_t(7));if(indexStart>limit)throw std::runtime_error("Compressed blob exceeds table");if(!compressed.empty()&&t.validRecordCount){int treeSize=minOff==INT_MAX?0:minOff;if(treeSize<0||size_t(treeSize)>t.compressedBytes)throw std::runtime_error("Invalid Huffman tree size");Huff h(treeSize/4);for(size_t n=0;n<h.child.size();n++){h.child[n][0]=b[blob+n*4];h.leaf[n][0]=b[blob+n*4+1];h.child[n][1]=b[blob+n*4+2];h.leaf[n][1]=b[blob+n*4+3];}for(auto&r:t.rows)for(size_t ci=0;ci<t.columns.size();ci++)if(t.columns[ci].type==NativeFieldType::ShortCompressedString||t.columns[ci].type==NativeFieldType::LongCompressedString){int off=std::get<int>(r.values[ci]);if(off<0){r.values[ci]=std::string();continue;}size_t at=blob+off;if(at>=blob+t.compressedBytes)throw std::runtime_error("Compressed string offset out of bounds");size_t len=t.columns[ci].type==NativeFieldType::ShortCompressedString?b[at++]:u16(b,at,false),data=at+(t.columns[ci].type==NativeFieldType::ShortCompressedString?0:2);r.values[ci]=h.read(b,data,len);}
    } for(auto& row:t.rows)row.originalValues=row.values; size_t ix=indexStart;for(unsigned z=0;z<indexes;z++){if(ix+8>limit)throw std::runtime_error("Index out of bounds");unsigned cc=b[ix+4];ix+=8+size_t(cc)*8;if(ix>limit)throw std::runtime_error("Index columns out of bounds");}t.recordsCrcOffset=ix;if(ix+4>limit)throw std::runtime_error("Record CRC out of bounds");db.tables.push_back(std::move(t));
  }return db;
}

EditResult DatabaseEngine::stageEdit(NativeDatabase& database,const std::string& tableName,size_t rowIndex,const std::string& fieldName,const std::string& textValue) const {
  auto equal=[](const std::string&a,const std::string&b){return a.size()==b.size()&&std::equal(a.begin(),a.end(),b.begin(),[](unsigned char x,unsigned char y){return std::tolower(x)==std::tolower(y);});};
  auto table=std::find_if(database.tables.begin(),database.tables.end(),[&](const NativeTable&t){return equal(t.name,tableName)||equal(t.shortName,tableName);});
  if(table==database.tables.end())return {false,"Table not found"}; if(rowIndex>=table->rows.size())return {false,"Row index out of range"};
  auto col=std::find_if(table->columns.begin(),table->columns.end(),[&](const NativeColumn&c){return equal(c.name,fieldName)||equal(c.shortName,fieldName);});
  if(col==table->columns.end())return {false,"Field not found"};const size_t index=static_cast<size_t>(col-table->columns.begin());
  try { if(col->type==NativeFieldType::Integer){int value{};const auto [end,ec]=std::from_chars(textValue.data(),textValue.data()+textValue.size(),value);if(ec!=std::errc{}||end!=textValue.data()+textValue.size())return {false,"Integer value required"};if(value<col->rangeLow||value>col->rangeHigh)return {false,"Value outside metadata range"};table->rows[rowIndex].values[index]=value;}
    else if(col->type==NativeFieldType::Float){char* end{};float value=std::strtof(textValue.c_str(),&end);if(end!=textValue.c_str()+textValue.size())return {false,"Float value required"};table->rows[rowIndex].values[index]=value;}
    else if(col->type==NativeFieldType::String){if(textValue.size()>=static_cast<size_t>(col->depth/8))return {false,"Text exceeds fixed field capacity"};table->rows[rowIndex].values[index]=textValue;}
    else if(col->type==NativeFieldType::ShortCompressedString||col->type==NativeFieldType::LongCompressedString){if(table->structuralChanged)return {false,"Compressed text cannot be changed in a table with pending insert/delete"};table->rows[rowIndex].values[index]=textValue;}
    else return {false,"Unsupported native field type"}; }
  catch(const std::exception&e){return {false,e.what()};}return {true,"Edit staged"};
}

EditResult DatabaseEngine::duplicateRow(NativeDatabase& database,const std::string& tableName,size_t rowIndex) const {
  auto equal=[](const std::string&a,const std::string&b){return a.size()==b.size()&&std::equal(a.begin(),a.end(),b.begin(),[](unsigned char x,unsigned char y){return std::tolower(x)==std::tolower(y);});};
  auto table=std::find_if(database.tables.begin(),database.tables.end(),[&](const NativeTable&t){return equal(t.name,tableName)||equal(t.shortName,tableName);});
  if(table==database.tables.end())return {false,"Table not found"};if(rowIndex>=table->rows.size())return {false,"Row index out of range"};if(table->recordCount>=65535)return {false,"Table has reached the T3DB record limit"};
  table->rows.insert(table->rows.begin()+static_cast<std::ptrdiff_t>(rowIndex+1),table->rows[rowIndex]);++table->recordCount;++table->validRecordCount;table->structuralChanged=true;
  return {true,"Record duplicated; change all key fields before saving"};
}

EditResult DatabaseEngine::deleteRow(NativeDatabase& database,const std::string& tableName,size_t rowIndex) const {
  auto equal=[](const std::string&a,const std::string&b){return a.size()==b.size()&&std::equal(a.begin(),a.end(),b.begin(),[](unsigned char x,unsigned char y){return std::tolower(x)==std::tolower(y);});};
  auto table=std::find_if(database.tables.begin(),database.tables.end(),[&](const NativeTable&t){return equal(t.name,tableName)||equal(t.shortName,tableName);});
  if(table==database.tables.end())return {false,"Table not found"};if(rowIndex>=table->rows.size())return {false,"Row index out of range"};if(table->recordCount==0||table->validRecordCount==0)return {false,"Table has no record to delete"};
  table->rows.erase(table->rows.begin()+static_cast<std::ptrdiff_t>(rowIndex));--table->recordCount;--table->validRecordCount;table->structuralChanged=true;
  return {true,"Record deleted in memory; related records are not changed automatically"};
}

EditResult DatabaseEngine::deleteRowWithRelationships(NativeDatabase& database,const std::string& tableName,size_t rowIndex) const {
  auto equal=[](const std::string&a,const std::string&b){return a.size()==b.size()&&std::equal(a.begin(),a.end(),b.begin(),[](unsigned char x,unsigned char y){return std::tolower(x)==std::tolower(y);});};
  auto parent=std::find_if(database.tables.begin(),database.tables.end(),[&](const NativeTable&t){return equal(t.name,tableName)||equal(t.shortName,tableName);});
  if(parent==database.tables.end())return {false,"Table not found"};if(rowIndex>=parent->rows.size())return {false,"Row index out of range"};
  auto integer=[](const CellValue&v,int&out){if(const auto p=std::get_if<int>(&v)){out=*p;return true;}return false;};
  struct Clear { NativeTable* table; size_t row,column; int value; };struct Erase { NativeTable* table; std::vector<size_t> rows; };
  std::vector<Clear> clears;std::vector<Erase> erases;size_t removed=0,cleared=0;
  for(auto& child:database.tables){Erase erase{&child,{}};for(size_t ci=0;ci<child.columns.size();++ci){const auto&column=child.columns[ci];if(!equal(column.foreignTable,parent->name))continue;size_t parentColumn=parent->columns.size();for(size_t pc=0;pc<parent->columns.size();++pc)if(equal(parent->columns[pc].name,column.name)){parentColumn=pc;break;}if(parentColumn==parent->columns.size())for(size_t pc=0;pc<parent->columns.size();++pc)if(parent->columns[pc].key){parentColumn=pc;break;}if(parentColumn==parent->columns.size())continue;int target{};if(!integer(parent->rows[rowIndex].values[parentColumn],target))continue;
      for(size_t ri=0;ri<child.rows.size();++ri){int value{};if(!integer(child.rows[ri].values[ci],value)||value!=target)continue;const bool association=child.name.size()>=5&&child.name.ends_with("links");if(association||column.key){erase.rows.push_back(ri);continue;}int sentinel=column.rangeLow<=-1?-1:(column.rangeLow<=0?0:INT_MIN);if(sentinel==INT_MIN)return {false,"Cannot delete: "+child.name+"."+column.name+" references this record and has no null sentinel"};clears.push_back({&child,ri,ci,sentinel});}
    }if(!erase.rows.empty()){std::sort(erase.rows.begin(),erase.rows.end());erase.rows.erase(std::unique(erase.rows.begin(),erase.rows.end()),erase.rows.end());removed+=erase.rows.size();erases.push_back(std::move(erase));}}
  for(const auto&clear:clears){clear.table->rows[clear.row].values[clear.column]=clear.value;++cleared;}
  for(auto&erase:erases){for(auto it=erase.rows.rbegin();it!=erase.rows.rend();++it){erase.table->rows.erase(erase.table->rows.begin()+static_cast<std::ptrdiff_t>(*it));--erase.table->recordCount;--erase.table->validRecordCount;}erase.table->structuralChanged=true;}
  parent->rows.erase(parent->rows.begin()+static_cast<std::ptrdiff_t>(rowIndex));--parent->recordCount;--parent->validRecordCount;parent->structuralChanged=true;
  return {true,"Record deleted with "+std::to_string(removed)+" dependent link/child record(s) removed and "+std::to_string(cleared)+" reference(s) cleared"};
}

std::vector<std::string> DatabaseEngine::validateIntegrity(const NativeDatabase& database) const {
  auto equal=[](const std::string&a,const std::string&b){return a.size()==b.size()&&std::equal(a.begin(),a.end(),b.begin(),[](unsigned char x,unsigned char y){return std::tolower(x)==std::tolower(y);});};
  auto text=[](const CellValue& v){if(const auto i=std::get_if<int>(&v))return std::to_string(*i);if(const auto f=std::get_if<float>(&v))return std::to_string(*f);return std::get<std::string>(v);};
  std::vector<std::string> issues;
  for(const auto&t:database.tables){std::vector<size_t> keys;for(size_t c=0;c<t.columns.size();++c)if(t.columns[c].key)keys.push_back(c);if(t.structuralChanged&&!keys.empty()){std::set<std::string> seen;for(size_t r=0;r<t.rows.size();++r){std::string composite;for(auto c:keys){composite+=text(t.rows[r].values[c]);composite.push_back('\x1f');}if(!seen.insert(composite).second)issues.push_back(t.name+" row "+std::to_string(r)+": duplicate key");}}
    if(!t.structuralChanged)continue;for(size_t c=0;c<t.columns.size();++c){const auto& column=t.columns[c];if(column.foreignTable.empty())continue;auto parent=std::find_if(database.tables.begin(),database.tables.end(),[&](const NativeTable& p){return equal(p.name,column.foreignTable);});if(parent==database.tables.end())continue;size_t parentField=parent->columns.size();for(size_t pc=0;pc<parent->columns.size();++pc)if(equal(parent->columns[pc].name,column.name)){parentField=pc;break;}if(parentField==parent->columns.size())for(size_t pc=0;pc<parent->columns.size();++pc)if(parent->columns[pc].key){parentField=pc;break;}if(parentField==parent->columns.size())continue;std::set<std::string> values;for(const auto& row:parent->rows)values.insert(text(row.values[parentField]));for(size_t r=0;r<t.rows.size();++r){const auto value=text(t.rows[r].values[c]);if(value=="-1"||value=="0"||value.empty())continue;if(!values.contains(value))issues.push_back(t.name+" row "+std::to_string(r)+"."+column.name+": missing "+column.foreignTable+" key "+value);}}
  }
  return issues;
}

void DatabaseEngine::saveT3dbCopy(const NativeDatabase& database,const std::filesystem::path& outputPath) const {
  auto source=database.bytes;
  for(const auto&t:database.tables)if(!t.structuralChanged)rewriteCompressedStrings(source,database,t);
  struct WrittenTable { size_t tableCrc{}, recordsCrc{}; };
  const size_t directoryBase=database.shortNamesCrcOffset+4;
  if(directoryBase>source.size())throw std::runtime_error("Invalid database directory");
  std::vector<unsigned char> rebuilt(source.begin(),source.begin()+directoryBase);std::vector<WrittenTable> written;written.reserve(database.tables.size());
  for(size_t ti=0;ti<database.tables.size();++ti){const auto&t=database.tables[ti];if(t.rows.size()!=t.validRecordCount)throw std::runtime_error("Table row count is inconsistent");if(t.tableOffset>t.recordDataOffset||t.recordsCrcOffset+4>t.tableEndOffset||t.tableEndOffset>source.size())throw std::runtime_error("Table boundaries are invalid");
    // The original record count is encoded by the source layout; structural edits change it only in the model.
    const size_t sourceRecordCount=(t.tableEndOffset>t.recordDataOffset? [&](){ return size_t(u16(source,t.tableOffset+16,database.littleEndian)); }():0);
    const size_t sourceValidCount=size_t(u16(source,t.tableOffset+18,database.littleEndian));
    if(sourceValidCount>sourceRecordCount)throw std::runtime_error("Invalid source valid-record count");
    const size_t blobStart=t.recordDataOffset+size_t(t.recordSize)*sourceRecordCount;
    if(blobStart>t.recordsCrcOffset)throw std::runtime_error("Invalid source record area");
    const size_t newStart=rebuilt.size(),directoryEntry=directoryBase-4-database.tables.size()*8+ti*8;put32(rebuilt,directoryEntry+4,static_cast<uint32_t>(newStart-directoryBase),database.littleEndian);
    rebuilt.insert(rebuilt.end(),source.begin()+t.tableOffset,source.begin()+t.recordDataOffset);put16(rebuilt,newStart+16,static_cast<uint16_t>(t.recordCount),database.littleEndian);put16(rebuilt,newStart+18,static_cast<uint16_t>(t.validRecordCount),database.littleEndian);
    for(const auto&r:t.rows){if(r.values.size()!=t.columns.size()||r.originalValues.size()!=t.columns.size()||r.originalBytes.size()!=t.recordSize)throw std::runtime_error("Row has invalid value count");auto rec=r.originalBytes;
      for(size_t ci=0;ci<t.columns.size();ci++){const auto&c=t.columns[ci];const size_t byte=c.bitOffset<0?rec.size():static_cast<size_t>(c.bitOffset/8);if((c.type==NativeFieldType::Integer&&(c.depth<=0||byte+(static_cast<size_t>(c.bitOffset%8+c.depth)+7)/8>rec.size()))||((c.type==NativeFieldType::Float||c.type==NativeFieldType::ShortCompressedString||c.type==NativeFieldType::LongCompressedString)&&byte+4>rec.size())||(c.type==NativeFieldType::String&&(c.depth<=0||byte+static_cast<size_t>(c.depth/8)>rec.size())))throw std::runtime_error("Field descriptor exceeds record boundary");
        if(c.type==NativeFieldType::ShortCompressedString||c.type==NativeFieldType::LongCompressedString){if(!t.structuralChanged){const size_t p=t.recordDataOffset+r.sourceRecordIndex*size_t(t.recordSize)+byte;if(p+4>source.size())throw std::runtime_error("Compressed pointer is out of bounds");std::copy(source.begin()+p,source.begin()+p+4,rec.begin()+byte);}continue;}
        if(r.values[ci]==r.originalValues[ci])continue;if(c.type==NativeFieldType::Integer){auto v=std::get<int>(r.values[ci]);if(v<c.rangeLow||(c.depth<31&&uint64_t(v-c.rangeLow)>((1ULL<<c.depth)-1)))throw std::runtime_error("Integer exceeds field range");putBits(rec,c.bitOffset,c.depth,uint32_t(v-c.rangeLow),database.littleEndian);}else if(c.type==NativeFieldType::Float){put32(rec,byte,std::bit_cast<uint32_t>(std::get<float>(r.values[ci])),database.littleEndian);}else if(c.type==NativeFieldType::String){const auto&s=std::get<std::string>(r.values[ci]);const auto n=c.depth/8;if(s.size()>=n)throw std::runtime_error("Fixed string exceeds field capacity");std::fill(rec.begin()+byte,rec.begin()+byte+n,static_cast<unsigned char>(0));std::memcpy(rec.data()+byte,s.data(),s.size());}}
      rebuilt.insert(rebuilt.end(),rec.begin(),rec.end());}
    // T3DB keeps inactive capacity records after the valid sequence. They are not represented as rows,
    // but must survive a table rebuild so locale tables and reserved-capacity tables remain byte-valid.
    const size_t inactiveRecords=sourceRecordCount-sourceValidCount;
    if(t.rows.size()+inactiveRecords!=t.recordCount)throw std::runtime_error("Structural record count does not preserve inactive capacity");
    const size_t inactiveStart=t.recordDataOffset+sourceValidCount*size_t(t.recordSize);
    rebuilt.insert(rebuilt.end(),source.begin()+inactiveStart,source.begin()+blobStart);
    rebuilt.insert(rebuilt.end(),source.begin()+blobStart,source.begin()+t.recordsCrcOffset);const size_t recordsCrc=rebuilt.size();rebuilt.insert(rebuilt.end(),4,0);written.push_back({newStart+32,recordsCrc});
  }
  const size_t logicalSize=rebuilt.size();if(database.encrypted)while(rebuilt.size()%16)rebuilt.push_back(0);put32(rebuilt,8,static_cast<uint32_t>(database.encrypted?logicalSize:rebuilt.size()),database.littleEndian);auto crc=[&](size_t from,size_t to,size_t at){put32(rebuilt,at,crcDb11(rebuilt,from,to-from),database.littleEndian);};crc(0,database.headerCrcOffset,database.headerCrcOffset);crc(database.headerCrcOffset+4,database.shortNamesCrcOffset,database.shortNamesCrcOffset);size_t sig=database.shortNamesCrcOffset+4;for(const auto&w:written){crc(sig,w.tableCrc,w.tableCrc);sig=w.tableCrc+4;crc(sig,w.recordsCrc,w.recordsCrc);sig=w.recordsCrc+4;}
  std::vector<unsigned char> out;if(database.encrypted){if(!encryptEngUs(rebuilt,out))throw std::runtime_error("Locale encryption failed: rebuilt length is not AES block aligned ("+std::to_string(rebuilt.size())+")");}else out=std::move(rebuilt);std::ofstream f(outputPath,std::ios::binary|std::ios::trunc);if(!f)throw std::runtime_error("Cannot create output database");f.write(reinterpret_cast<const char*>(out.data()),static_cast<std::streamsize>(out.size()));if(!f)throw std::runtime_error("Database write failed");
}
}
