# CM16 database source inventory

Located under `<FC26 tools>\cm16 source code\FifaLibrary16.dll\FifaLibrary`:

- `DbFile.cs` (15,523 B), `DbReader.cs` (3,895 B), `DbWriter.cs` (5,117 B)
- `Table.cs` (13,885 B), `Record.cs` (22,608 B), `TableDescriptor.cs` (8,213 B), `FieldDescriptor.cs` (3,781 B)
- `HuffmannTree.cs` (4,149 B), `PlayerNames.cs` (9,674 B), `PlayerName.cs` (1,782 B), `Player.cs` (196,226 B)
- `Language.cs` (17,795 B), `Localization.cs` (2,613 B)

`DbReader.PopIntegerPc` and `DbWriter.PushIntegerPc` use packed PC fields with range-low bias and bit-position state. `FieldDescriptor` preserves type, bit offset, short name and depth; compressed string types are 13/14. `Localization.cs` is application `.resx` UI localization, not FC26 `eng_us.DB` decryption. No AES implementation was found in this required CM16 set.

Verdict: reader/descriptors/Huffman are **FIFA16-SPECIFIC REFERENCE ONLY**; writer **DO NOT REUSE**; player names and language localization **FIFA16-SPECIFIC**.
