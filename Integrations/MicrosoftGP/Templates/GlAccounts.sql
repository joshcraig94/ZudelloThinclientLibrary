select 
'GLCodes' as 'object_type',
'{{connection z.connection_id}}' as 'connection_uuid',
ACTINDX as [data.id],
MNACSGMT as [data.haderAccount],                                                               
ACTDESCR as [data.name],
CONCAT('-',ACTNUMBR_1,ACTNUMBR_2,ACTNUMBR_3,ACTNUMBR_4,ACTNUMBR_5)as [data.code], --SQL Server Version > 16 use CONCAT_WS\
ACCTTYPE as [data.accountType]
from GL00100
where DEX_ROW_TS > 0 --'{{last_sync}}'
for JSON PATH, root('data')

--GL00105
--Select * from SY00300 Segment names
-- Segemnt descriptions GL40200