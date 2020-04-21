
--Vendor GP
select
'CONTACT' as 'object_type',
'{{connection z.connection_id}}' as 'connection_uuid',
VENDORID  as [data.id],  
VENDORID as  [data.code],
VENDNAME as [data.contact_name], 
VNDCHKNM as [data.tradingName],
CURNCYID as [data.currency],
''	as [data.email],
ADDRESS1 as [data.address], 
ADDRESS2 as [data.address1], 
CITY as [data.city],
STATE as [data.state],
ZIPCODE as [data.postalCode], 
COUNTRY as [data.country], 
PHNUMBR1 as [data.phone], 
TXRGNNUM AS [data.tax]
FROM 
  GP_DE.dbo.PM00200 
  where VENDORID = 'ACETRAVE0001' -- {{last_sync z.id }}
  for  JSON PATH, ROOT('data')