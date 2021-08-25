CREATE TABLE "public"."query" (
    query_id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    type VARCHAR(255) NOT NULL,
    text VARCHAR(255) NOT NULL,
    elapsed_milliseconds INT NOT NULL,
    ravendb_id INT NOT NULL UNIQUE,
    created_date TIMESTAMP WITHOUT TIME ZONE NOT NULL
);
ALTER TABLE "public"."query" ALTER COLUMN ravendb_id NULL;
CREATE INDEX query_created_date_idx ON query (created_date);

ALTER TABLE "public"."query" ADD COLUMN user_agent VARCHAR(255);
ALTER TABLE "public"."query" ADD COLUMN user_host_address VARCHAR(255);
ALTER TABLE "public"."query" ADD COLUMN browser_screen_pixels_height INT;
ALTER TABLE "public"."query" ADD COLUMN browser_screen_pixels_width INT;
ALTER TABLE "public"."query" ADD COLUMN result_date TIMESTAMP WITHOUT TIME ZONE NULL;
