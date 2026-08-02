# Four-engine locale and Huffman comparison

Current CM26 implements AES-256-CBC/no-padding using Windows BCrypt, decrypts the locale before parsing, and uses an MSB-first Huffman payload traversal. Its smoke test successfully parses two language tables and performs a locale copy edit/reload.

Local T3DbEngine has no demonstrated AES implementation in the audited core; its language services include external fallback paths. CM16 contains HuffmannTree/Language code but no required AES implementation. DBM Studio reads plaintext T3DB Huffman strings only and does not decrypt `eng_us.DB`.

Current AES key/IV verification and full locale linkage are **NOT TESTED in this audit**; no decoder defect is established.
