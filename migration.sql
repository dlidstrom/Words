-- permutation table:
UPDATE
    word
SET
    permutation = subquery.normalized
FROM (
    SELECT
        p.normalized,
        p.permutation
    FROM
        permutation p
        JOIN word w ON p.permutation = w.text
    WHERE
        w.permutation IS NULL
    LIMIT 100000) AS subquery
WHERE
    word.permutation IS NULL
    AND word.text = subquery.permutation;

SELECT
    COUNT(*) FILTER (
    WHERE w.permutation IS NULL) count_nulls
    ,
  COUNT(*) FILTER (
    WHERE w.permutation IS NOT NULL) count_not_nulls
FROM
    word w;

-- normalized table:
-- normalized original
UPDATE
    word
SET
    normalized = subquery.normalized
FROM (
    SELECT
        n.normalized,
        n.original
    FROM
        normalized n
        JOIN word w ON n.original = w.text
    WHERE
        w.normalized IS NULL
    LIMIT 100000) AS subquery
WHERE
    word.normalized IS NULL
    AND word.text = subquery.original;

SELECT
    COUNT(*) FILTER (
    WHERE w.normalized IS NULL) count_nulls
    ,
  COUNT(*) FILTER (
    WHERE w.normalized IS NOT NULL) count_not_nulls
FROM
    word w;

CREATE INDEX word_permutation_idx ON word (permutation);
CREATE INDEX word_normalized_idx ON word (normalized);
