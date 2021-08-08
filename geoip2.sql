create table geoip2_network (
  network cidr not null,
  geoname_id int,
  registered_country_geoname_id int,
  represented_country_geoname_id int,
  is_anonymous_proxy bool,
  is_satellite_provider bool,
  postal_code text,
  latitude numeric,
  longitude numeric,
  accuracy_radius int
);
create index on geoip2_network using gist (network inet_ops);

create table geoip2_location (
  geoname_id int not null,
  locale_code text not null,
  continent_code text not null,
  continent_name text not null,
  country_iso_code text,
  country_name text,
  subdivision_1_iso_code text,
  subdivision_1_name text,
  subdivision_2_iso_code text,
  subdivision_2_name text,
  city_name text,
  metro_code int,
  time_zone text,
  is_in_european_union bool not null,
  primary key (geoname_id, locale_code)
);

ALTER TABLE "public"."query" ALTER COLUMN "user_host_address" TYPE cidr using "user_host_address"::cidr;

CREATE OR REPLACE VIEW v_geolocation AS (
    WITH user_host_address AS (
        SELECT
            q.text,
            q.user_host_address,
            q.created_date
        FROM
            query q
        WHERE
            q.user_host_address IS NOT NULL
        ORDER BY q.created_date DESC
        LIMIT 1000)
        SELECT
            uha.text,
            uha.user_host_address,
            uha.created_date,
            net.network,
            net.geoname_id,
            net.registered_country_geoname_id,
            net.represented_country_geoname_id,
            net.is_anonymous_proxy,
            net.is_satellite_provider,
            net.postal_code,
            net.latitude,
            net.longitude,
            net.accuracy_radius,
            loc.locale_code,
            loc.continent_code,
            loc.continent_name,
            loc.country_iso_code,
            loc.country_name,
            loc.subdivision_1_iso_code,
            loc.subdivision_1_name,
            loc.subdivision_2_iso_code,
            loc.subdivision_2_name,
            loc.city_name,
            loc.metro_code,
            loc.time_zone,
            loc.is_in_european_union
        FROM
            user_host_address uha
            JOIN geoip2_network net ON (net.network >> uha.user_host_address)
            JOIN geoip2_location loc ON (net.geoname_id = loc.geoname_id
                    AND loc.locale_code = 'en'));
