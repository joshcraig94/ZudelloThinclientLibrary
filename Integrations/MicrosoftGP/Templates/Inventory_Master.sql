Select  
'ITEM' as 'object_type',
'{{connection z.connection_id}}' as 'connection_uuid',
ITEMNMBR as [data.id],
ITEMNMBR as [data.sku],
CASE when ITEMTYPE = 1 then 'true' else 'false' end as [data.isStock],
ITEMDESC as [data.description],
ALTITEM1 as [data.barcode.barcode1],
ALTITEM2 as [data.barcode.barcode2],
PRCHSUOM as [data.purcahseUom], 
SELNGUOM as [data.salesUom],

(SELECT 
	VENDORID as [id],
	VNDITNUM as [supplierCode],
	VNDITDSC as [description],
	Last_Originating_Cost as [latestCost],
	PRCHSUOM as [supplierPurchaseUom]
	from IV00103 as vendor_items
	where vendor_items.ITEMNMBR = item_master.ITEMNMBR
	for XML PATH, root('data')

) as [supplierCodes]C:\Users\joshc\Documents\GitHub\zudello-thinclient\MicrosoftGPConnector\Templates\SupplierInvoice.xml
from IV00101 as item_master
where ITEMNMBR = 'ACCS-CRD-12WH'
for JSON PATH, root('data')
--might need to add trim functions 

/*
select * from IV00101

select * from IV00103

5- no stock 
1- sales inventory 
4 -  misc chargers
3 -  kit
6 - Flat Free

select Purchase_Item_Tax_Schedu, 'Tax Options' = dbo.DYN_FUNC_Tax_Options(IV00101.TAXOPTNS) 
from IV00101 
left outer join IV00115 on IV00101.ITEMNMBR = IV00115.ITEMNMBR
 
 select * from Items

--IV00103 is supplier alternatives

select SELNGUOM from IV00101

*/