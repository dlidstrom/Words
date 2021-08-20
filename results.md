# Resultat <!-- omit in toc -->

- [When using RavenDB](#when-using-ravendb)
- [When using PostgreSQL, all in one tree](#when-using-postgresql-all-in-one-tree)
- [When using PostgreSQL, separate trees](#when-using-postgresql-separate-trees)

## When using RavenDB

```sql
SELECT
    round(avg(q.elapsed_milliseconds), 2) AS "avg",
    round(stddev(q.elapsed_milliseconds), 2) AS "stddev",
    round(avg(q.elapsed_milliseconds) + (2 * stddev(q.elapsed_milliseconds)), 2) AS "95%-tile",
    percentile_cont(0.5) WITHIN GROUP (ORDER BY q.elapsed_milliseconds) AS median,
    percentile_cont(0.95) WITHIN GROUP (ORDER BY q.elapsed_milliseconds) AS percentile_95
FROM
    query q
WHERE
    q.ravendb_id IS NOT NULL;
```

| avg    | stddev  | 95%-tile | median | percentile_95     |
| ------ | ------- | -------- | ------ | ----------------- |
| 860.89 | 1942.33 | 4745.55  | 129    | 4478.099999999991 |

## When using PostgreSQL, all in one tree

```sql
SELECT
    round(avg(q.elapsed_milliseconds), 2) AS "avg",
    round(stddev(q.elapsed_milliseconds), 2) AS "stddev",
    round(avg(q.elapsed_milliseconds) + (2 * stddev(q.elapsed_milliseconds)), 2) AS "95%-tile",
    percentile_cont(0.5) WITHIN GROUP (ORDER BY q.elapsed_milliseconds) AS median,
    percentile_cont(0.95) WITHIN GROUP (ORDER BY q.elapsed_milliseconds) AS percentile_95
FROM
    query q
WHERE
    q.ravendb_id IS NULL
    AND q.created_date < '2021-08-20';
```

| avg    | stddev | 95%-tile | median | percentile_95 |
| ------ | ------ | -------- | ------ | ------------- |
| 166.64 | 526.76 | 1220.17  | 71     | 385           |

## When using PostgreSQL, separate trees

```sql
SELECT
    round(avg(q.elapsed_milliseconds), 2) AS "avg",
    round(stddev(q.elapsed_milliseconds), 2) AS "stddev",
    round(avg(q.elapsed_milliseconds) + (2 * stddev(q.elapsed_milliseconds)), 2) AS "95%-tile",
    percentile_cont(0.5) WITHIN GROUP (ORDER BY q.elapsed_milliseconds) AS median,
    percentile_cont(0.95) WITHIN GROUP (ORDER BY q.elapsed_milliseconds) AS percentile_95
FROM
    query q
WHERE
    q.ravendb_id IS NULL
    AND q.created_date >= '2021-08-20';
```

| avg   | stddev | 95%-tile | median | percentile_95      |
| ----- | ------ | -------- | ------ | ------------------ |
| 479.4 | 777,91 | 2035,22  | 40.5   | 1762.8999999999987 |
