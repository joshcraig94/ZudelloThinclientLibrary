/*Set up scripts for implementation to run in storeman */
/*Copy Team ID*/
create temporary table X_temp_client_mappings  as
/*Copy Team ID*/
select * from client_mappings where team_id = 608;

/*Change Team ID to new Connection*/
update X_temp_client_mappings set team_id = 798;
/*generate new uuids*/
UPDATE X_temp_client_mappings set uuid = (SELECT UUID());
/*Get new ID's*/
SET @a = (select MAX(id) from client_mappings);
UPDATE X_temp_client_mappings SET id = @a:=@a+1;
/*insert into client_mappings*/
insert into client_mappings select  * from X_temp_client_mappings;

drop table X_temp_client_mappings;


more scripts to copy into presets 

/*Set up scripts for implementation to run in storeman */
/*Copy Team ID*/
select * from X_temp_client_mappings
/*Copy Team ID*/
alter table X_temp_client_mappings drop column connection_id
select * from preset_client_mappings

/*generate new uuids*/
UPDATE X_temp_client_mappings set uuid = (SELECT UUID());
/*Get new ID's*/
SET @a = (select MAX(id) from preset_client_mappings);
UPDATE X_temp_client_mappings SET id = @a:=@a+1;
/*insert into client_mappings*/
insert into preset_client_mappings select  * from X_temp_client_mappings;

drop table X_temp_client_mappings;
