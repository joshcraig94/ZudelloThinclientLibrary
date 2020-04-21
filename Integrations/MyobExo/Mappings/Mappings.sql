
/* Get supplier*/
Select top 1

'CONTACT' as 'object_type',
'render this from templating language' as 'connection_uuid',
ACCNO as [data.id], 
ALPHACODE as [data.code],
NAME as [data.contact_name],
NAME as [data.tradingName],
cur.CURRCODE as [data.currency],
EMAIL as [data.email],
ADDRESS1 as [data.address],
ADDRESS2 as [data.city],
ADDRESS3 as [data.country],
POST_CODE as [data.postalCode],
PHONE as [data.phone],
TAXREG AS [data.tax],
'1'  as [data.isSupplier],
'0' as [data.isCustomer]   

from CR_ACCS cr
left join CURRENCIES cur on cur.CURRENCYNO = cr.CURRENCYNO
where Accno = 0
for JSON PATH, ROOT('data')


                /*Select the data from JSON
                string JSONSelect = String.Format(" SELECT x.[KEY], x.[Value] FROM OPENJSON('{0}', '$.data') as x where x.[KEY] in ({1}) ",
                    readContents, JsonSelect);


                string SqlInsert = string.Format(@" SET  IDENTITY_INSERT DBO.{0} ON; " +
                                                        "INSERT INTO  {0} (SEQNO, {1}) " +
                                                        "SELECT SEQNO = (SELECT MAX(SEQNO)+1 FROM {0}), {1} FROM ({2}) as d" +
                                                        " Pivot (max([value]) for [key] in ({1})) piv", TableName, insertInto, JSONSelect);
														*/


/*Purchase Orders*/

select
'Purchase Order' as 'object_type',
'render this from templating language' as 'connection_uuid',
poh.SEQNO as [data.id],
br.BCODE as [data.Branch],
cr.NAME as [data.supplier],
poh.REFERENCE as [data.supplierreference],
poh.ORDERDATE as [data.orderdate],
poh.DUEDATE as [data.orderduredate],
poh.SEQNO as [data.ordernumber], 
poh.SEQNO as [data.purchaseordernumber],
Case 
	when poh.status = 0 then 'Open' 
	when  poh.status = 1 then 'Partial'
	when poh.status = 2 then 'Closed'
	end as [data.status],

poh.EXCHRATE as [data.exchangerate],
poh.SUBTOTAL as [data.subtotal],
poh.TAXTOTAL as [data.taxamount],
poh.ORDTOTAL as [data.total],
poh.ADDRESS1 as [data.deliverydescription],
poh.ADDRESS2 as [data.deliveryaddr1],
poh.ADDRESS3 as [data.deliverysuburb],

   ( select
        pol.SEQNO as [internaldocnum],
        pol.POLINEID as [linenumber],
        pol.STOCKCODE as [item],
		(Select case when status = 'L' then 'false' else 'true' end from stock_items where stockcode = pol.STOCKCODE ) as [isStock],
		pol.LOCATION as [location],
		pol.DESCRIPTION as [description],
		pol.ORD_QUANT as [orderquantity],
		pol.SUP_QUANT as [receiptquantity],
		pol.INV_QUANT as [invoicedquanity],
		pol.UNITPRICE as [unitprice],
		pol.LINETOTAL as [linetotal],
		pol.TAXRATE as [taxrate],
		pol.TAXRATE_NO as [taxratenumber],
		pol.JOBNO as [jobnumber],
		pol.BATCHCODE as [batchcode],
		pol.SUPPLIERCODE as [suppliercode]
		from PURCHORD_LINES pol
		WHERE pol.HDR_SEQNO = poh.SEQNO
        FOR JSON PATH
    ) AS [Lines]

	   


from PURCHORD_HDR poh
join CR_ACCS cr on cr.ACCNO = poh.ACCNO
join BRANCHES br on br.BRANCHNO = poh.BRANCHNO

where poh.seqno = 10082

FOR JSON PATH, Root('data')



  
Select 
'Supplier Invoice' as 'object_type',
'render this from templating language' as 'connection_uuid',
crt.SEQNO as [data.id],
br.BCODE as [data.Branch],
cr.NAME as [data.supplier],
crt.REF1 as [data.supplierreference],
crt.REF2 as [data.supplierreference2],
crt.TRANSDATE as [data.invoicedate],
crt.DUEDATE as [data.invoiceduedate],
crt.INVNO as [data.invoicenumber], 

Case 
	when crt.Allocated = 0 then 'Open' 
	when  crt.Allocated = 1 then 'Partial'
	when crt.Allocated = 2 then 'Closed'
	end as [data.status],

crt.EXCHRATE as [data.exchangerate],
crt.SUBTOTAL as [data.subtotal],
crt.TAXTOTAL as [data.taxamount],
crt.AMOUNT as [data.total],

   ( select
        crl.SEQNO as [internaldocnum],
        crl.CRINVLINEID as [linenumber],
        crl.STOCKCODE as [item],
		(Select case when status = 'L' then 'false' else 'true' end from stock_items where stockcode = crl.STOCKCODE ) as [isStock],
		crl.LOCATION as [location],
		crl.DESCRIPTION as [description],
		crl.QUANTITY as [quantity],		
		crl.UNITPRICE as [unitprice],
		crl.UNITPRICE_INCTAX as [unitpriceinc],
		crl.LINETOTAL as [linetotal],
		crl.LINETOTAL_INCTAX as [linetotalinc],
		crl.LINETOTAL_TAX as [linetotaltax],
		crl.DISCOUNT as [discount],
		crl.DISCOUNTAMT as [discountamount],
		crl.DISCOUNTPCT as [discountpct],
		crl.TAXRATE as [taxrate],
		crl.TAXRATE_NO as [taxratenumber],
		crl.JOBNO as [jobnumber],
		crl.BATCHCODE as [batchcode]	
		from CR_INVLINES crl
		WHERE crl.HDR_SEQNO = crt.SEQNO
        FOR JSON PATH
    ) AS [Lines]



from CR_TRANS crt
join CR_ACCS cr on cr.ACCNO = crt.ACCNO
join BRANCHES br on br.BRANCHNO = crt.BRANCHNO
where seqno = 274

FOR JSON PATH, Root('data')

select * from CR_INVLINES
where HDR_SEQNO = 274






/*TEMPLATING LANGUAGE SQL */

Select 
'CONTACT' as 'object_type',
{{connection z.connection_id}} as 'connection_uuid',
ACCNO as [data.id], 
ALPHACODE as [data.code],
NAME  as [data.contact_name],
NAME  as [data.tradingName],
cur.CURRCODE as [data.currency],
EMAIL as [data.email],
ADDRESS1 as [data.address],
ADDRESS2 as [data.city],
ADDRESS3 as [data.country],
POST_CODE as [data.postalCode],
PHONE as [data.phone],
TAXREG AS [data.tax],
'1'  as [data.isSupplier],
'0' as [data.isCustomer]   

from CR_ACCS cr
left join CURRENCIES cur on cur.CURRENCYNO = cr.CURRENCYNO
where LAST_UPDATED > {{last_sync z.id }}
for JSON PATH, ROOT('data')




/*fETCH iTEMS*/

Select 
'ITEM' as 'object_type',
{{connection z.connection_id}} as 'connection_uuid',
Stockcode as [data.id], 
Stockcode as [data.sku], 
case when si.status = 'L' then 'false' else 'true' end as [data.isStock],
DESCRIPTION as [data.description],
sg.GROUPNAME as [data.StockGroup],
si.BARCODE1 as [data.Barcode.Barcode1],
si.BARCODE2 as [data.Barcode.Barcode2],
si.BARCODE3 as [data.Barcode.Barcode3],
si.Suppliercost as [data.purchaseprice], 
si.pack as [data.uom],
si.PQTY as [data.uomQty],
(SELECT 
		ACCNO as [id],
		SUPPLIERCODE as [supplierCode],
		DESCRIPTION as [description],
		LATESTCOST as [LatestCost],
		ECONORDERQTY as [EOQ], 
		PURCHPACKQUANT as [purchasePackQty],
		PURCHPACKPRICE as [purchasePackPrice],
		DISCOUNT as [DISCOUNT]

		from 
		SUPPLIER_STOCK_ITEMS
		where STOCKCODE = si.STOCKCODE

     FOR JSON PATH
    ) AS [SupplierCodes]
from STOCK_ITEMS si
join STOCK_GROUPS sg on sg.GROUPNO = si.STOCKGROUP
where si.LAST_UPDATED > {{last_sync z.id }}
for JSON PATH, ROOT('data')


/* FETCH TAX RATES */

Select 
--will need to hash these so they dont sync up again
'TaxRates' as 'object_type',
{{connection z.connection_id}} as 'connection_uuid',
seqno as [data.id],
SHORTNAME as [data.code],
NAME as [data.name],
Rate as [data.Rate]

from TAX_RATES

FOR JSON PATH, Root('data')


/* FETCH GL ACCOUNTS AND SUBS */

select 
'ACCOUNTS' as 'object_type',
{{connection z.connection_id}} as 'connection_uuid',
gl.ACCNO as [data.id],
gl.ACCNO as [data.code],
gl.name as [data.name],
( select
		gls.SUBACCNO as [subAccount],
		gls.name as [name],
		gls.FULLACCOUNT as [fullaccount]
        
		from GLSUBACCS gls
		WHERE gl.ACCNO = gls.ACCNO
        FOR JSON PATH
    ) AS [SubAccs]

from GLACCS gl
where gl.LAST_UPDATED > {{last_sync z.id }}
FOR JSON PATH, Root('data')



/*FETCH LOCATIONS WITH WAREHOUSE */

DECLARE @branchNo INT = (Select MIN(BRANCHNO) from BRANCHES)
WHILE @branchNo < (Select MAX(BRANCHNO) from BRANCHES)
BEGIN

select 
'Locations' as 'object_type',
{{connection z.connection_id}} as 'connection_uuid',
LOCNO as [data.id],
LCODE as [data.code],
LNAME as [data.name],
CONCAT(DELADDR1,',', DELADDR2) as [data.adddress],
@branchNo as [data.subsidiary]

from STOCK_LOCATIONS
where ISACTIVE = 'Y'
 for JSON PATH, ROOT('data')
   SET @branchNo = @branchNo + 1;
END;



/* FETCH BRANCHES */

select 
'subsidiary' as 'object_type',
{{connection z.connection_id}} as 'connection_uuid',
BRANCHNO as [data.id],
BCODE as [data.code],
BRANCHNAME as [data.name],
'' as [data.adddress],
'' as [data.parent]

from BRANCHES

FOR JSON PATH, Root('data')


/* FETCH CURRENCY */

select 
'Currency' as 'object_type',
{{connection z.connection_id}} as 'connection_uuid',
cur.CURRENCYNO as [data.id],
cur.CURRCODE as [data.iso],
cur.CURRNAME as [data.name],
cr.TRANSDATE as [data.effectivedate],
cr.NEWBUYRATE as [data.buyrate],
cr.NEWSELLRATE as [data.sellrate]
from CURRENCIES cur 
left join CURRENCY_RATECHANGES cr on cr.CURRENCYNO = cur.CURRENCYNO

where cr.TRANSDATE > {{last_sync z.id }}
FOR JSON PATH, Root('data')


select 
'Locations' as 'object_type',
'{{connection z.connection_id}}' as 'connection_uuid',
BRANCHNO + 1 as [data.id],
BCODE as [data.code],
BRANCHNAME as [data.name],
'' as [data.adddress],
'' as [data.parent]

from BRANCHES
FOR XML PATH, Root('data')