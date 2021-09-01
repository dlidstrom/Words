-- \copy word(text) from 'C:\Programming\word_permutations.csv' with (format csv, header, encoding 'utf8', delimiter ';');
CREATE TABLE "public"."word" (
    word_id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    text VARCHAR(255) NOT NULL
);

ALTER TABLE "public"."word" ADD COLUMN "score" integer NOT NULL DEFAULT 0;
ALTER TABLE "public"."word" ADD COLUMN "permutation" TEXT NULL;
ALTER TABLE "public"."word" ADD COLUMN "normalized" TEXT NULL;
