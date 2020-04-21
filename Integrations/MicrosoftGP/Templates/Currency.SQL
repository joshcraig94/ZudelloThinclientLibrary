select 
'Currency' as 'object_type',
'{{connection z.connection_id}}' as 'connection_uuid',   

CURNCYID as [data.id],  --maybe user CURRNIDX for ID? 
ISOCURRC as [data.iso],
CRNCYDSC as [data.name],
DECSYMBL as [data.precision]

 
FROM DYNAMICS.dbo.MC40200

for JSON PATH, root('data')

--select * from  DYNAMICS.dbo.MC40300 cant find rates