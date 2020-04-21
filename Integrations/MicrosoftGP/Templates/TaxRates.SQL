select 
'TaxRates' as 'object_type',
'{{connection z.connection_id}}' as 'connection_uuid',


DEX_ROW_ID as [data.id],  
TAXDTLID as [data.code],
TXDTLDSC as [data.name],
TXDTLPCT as [data.Rate]

 
FROM GP_DE.dbo.TX00201
for JSON PATH, root('data')


