# Four-engine descriptor comparison

Current CM26 and DBM Studio read table field directories from physical T3DB records (type, bit offset, short name, depth) and map names from XML. Local T3DbEngine reads XML metadata and table headers; CM16 descriptors preserve the same essential field information.

The current parser loaded 279 tables and 360,298 rows in its smoke test. A table-by-table four-way comparison was not executed because local-reference files are different physical versions and the requested harness does not yet exist. Thus every per-table discrepancy is **NOT TESTED**, rather than inferred.

No protected parser change was made.
