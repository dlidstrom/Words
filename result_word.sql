CREATE TABLE "public"."result_word" (
    result_word_id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    query_id INT NOT NULL REFERENCES "public"."query",
    word_id INT NOT NULL REFERENCES "public"."word"
);

CREATE UNIQUE INDEX result_word_query_word ON "public"."result_word"(query_id, word_id);
