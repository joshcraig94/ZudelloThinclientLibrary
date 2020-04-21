
/** Insert Supplier */
/* Mappings for Zudell*/
Declare @name as varchar(60) = 'Josh21s221'
,@address1 varchar(30) = '',
 @address2 varchar(30) = '',
 @address3 varchar(30) = '',   
 @address4  varchar(30) = '',
 @address5 varchar(30) = '',
 @phone  varchar(30) = '',
 @email  varchar(60) = '',
 @taxReg varchar(30) = ''   
 /**End Mappings */

 declare @createSupplier int = 0
 begin 
  set @createSupplier =  (select COUNT(*) from CR_ACCS where name =  @name)
 end 

BEGIN

if @createSupplier < 1 

BEGIN

Begin Tran Create_Supplier



INSERT INTO [dbo].[CR_ACCS]
           ([NAME]
           ,[ADDRESS1]
           ,[ADDRESS2]
           ,[ADDRESS3]
           ,[ADDRESS4]
           ,[ADDRESS5]
           ,[PHONE]         
           ,[EMAIL]
		   ,[TAXREG])           
     VALUES
           (@name,@address1,@address2,@address3,@address4,@address5,@phone,@email,@taxReg  		   
		   )	
	
Commit Tran  Create_Supplier
END
else 
Begin
RAISERROR('Duplicate Supplier',16,1)
end
END 

/************************************************/



/****Header Records *****************************************/
declare @seqno int = (select max(seqno)+1 from cr_trans),
@branchno int = '' ,
@accno int = 0,
@transdate datetime = getdate(),
@dueDate datetime = null,
@name varchar = '' ,
@invno varchar(20)= 'TaxOnLines', 
@transtype int = 1,
@exchrate float = 1,
@subtotal float = 0,
@taxtotal float = 0,
@period_seqno int = 0

/***********************************************************/




 declare @createInvoice int = 0 , @error int = 0
 begin 
  set @createInvoice =  (select COUNT(*) from CR_TRANS where INVNO =  @invno and ACCNO = @accno)
 end 

 BEGIN 

 if  @createInvoice < 1

	BEGIN

	BEGIN TRAN Create_Invoice



		SET IDENTITY_INSERT  CR_TRANS ON
		SET @period_seqno = (SELECT MAX(SEQNO)
								from period_status ps 
								where ps.STARTDATE <=  @transdate and ps.STOPDATE >= EOMONTH(@transdate) and ps.LEDGER = 'C')

		INSERT INTO [dbo].[CR_TRANS]
           (
		     [SEQNO] ,
			 [BRANCHNO] ,
			 [ACCNO],		   
			 [TRANSTYPE],
		     [TRANSDATE],
			 [DUEDATE],
			 [INVNO],
			 [EXCHRATE],
			 [SUBTOTAL],
			 [TAXTOTAL],
			 [PERIOD_SEQNO]		   
			)
			 VALUES
		    (
			  @seqno,
			  @branchno,
			  @accno,
			  @transtype,
			  @transdate,
			  ISNULL(@dueDate,@transdate),-- might need to put into a function to calcuate the default terms from the supplier card
			  @invno,
			  @exchrate,
			  @subtotal,
			  @taxtotal,
			  @period_seqno
		  
			  )  
			SET IDENTITY_INSERT  CR_TRANS OFF

			commit tran Create_Invoice
		
		END
	
	else 
			Begin
				RAISERROR('Duplicate Invioce',16,1)
				set @error = 1
				
			end



	BEGIN 

	if @error <> 1 

	Begin

	BEGIN TRAN Create_Invoice

/** Templating Language to loop for each and set these variables*/
/***********Lines******************************************/


declare @laccno int = @accno,
		@linvno varchar(20) = @invno,     
		@lhdr_seqno int = @seqno,
		@lstockcode varchar(23) = '00-03720-00',
		@llinked_stockcode varchar(23) = '00-03720-00',	 -- must be used for GL and has to be the same as stockcode otherwise issues when user posts to Gl
		
		@ldescription varchar(23),
		@ltaxrate float = 10,
		@lquantity float = 1,
		@llinked_qty float = 1,-- must be used for GL and has to be the same as stockcode otherwise issues when user posts to Gl

		@lunitprice float = 10,
		@ldiscount float = 0,
		@ldiscountamt float = 0,
		@ldiscountpct float = 0,



		/**Scrip Variables**/

		@rounding int = (Select top 1 Convert(int,fieldvalue)  FROM PROFILE_VALUES  where FIELDNAME = 'COSTPRDECIMALS'),

	
		
		@llocation int = 1 ,
		@lcodeType varchar = 'G'	,
		@llinetax_overridden char = 'N',
		@ltaxrate_no int = 32,
		@lline_source int = 0			
		Declare @lunitprice_inctax float = (Select 
													CASE
														WHEN @ltaxrate > 0 then Round(SUM(@lunitprice*(@ltaxrate/100)+ @lunitprice),@rounding)
														ELSE @lunitprice END )-- will need to add in logic for discounting rates later. 
		
		Declare @llinetotal_tax float = (Select 
													CASE
														WHEN @ltaxrate > 0 then Round(SUM(@lunitprice*@lquantity) *(@ltaxrate/100),@rounding)
														ELSE 0 END )-- will need to add in logic for discounting rates later. 






/*******************************************************/

	INSERT INTO [dbo].[CR_INVLINES](
           [ACCNO],
		   [INVNO],
		   [HDR_SEQNO],
		   [STOCKCODE],
		   [LINKED_STOCKCODE],
		   [LINKED_QTY],
		   [DESCRIPTION],
		   [QUANTITY],
		   [UNITPRICE],
		   [TAXRATE],
		   [DISCOUNT],
		   [DISCOUNTAMT],
		   [DISCOUNTPCT],
		   [LOCATION],
		   [CODETYPE],
		   [LINETAX_OVERRIDDEN],
		   [TAXRATE_NO],
		   [LINE_SOURCE],
		   [UNITPRICE_INCTAX],
		   [LINETOTAL_TAX]
		   )
     VALUES
           (@laccno,
			@linvno,     
			@lhdr_seqno,
			@lstockcode,
			@llinked_stockcode,
			@llinked_qty,
			@ldescription,
			@lquantity,
			@lunitprice,
			@ltaxrate,
			@ldiscount ,
			@ldiscountamt,
			@ldiscountpct,
			@llocation,
			@lcodeType,
			@llinetax_overridden,
			@ltaxrate_no,
			@lline_source,
			@lunitprice_inctax,
			@llinetotal_tax

		)

		Commit TRAN Create_Invoice
         
		 END

	END
END 



/** Insert Stock items */
/* Mappings for Zudell*/
Declare @stockcode as varchar(30) = 'TESTItem'
,@Description varchar(30) = 'Test',
-- @stockgroup int = '',
 @status char(1) = 'S'   

 /**End Mappings */



Begin Tran Create_stockItems



INSERT INTO [dbo].[STOCK_ITEMS]
           ([STOCKCODE]
           ,[DESCRIPTION]          
           ,[STATUS])          
     VALUES
           (@stockcode,@Description,@status 		   
		   )	
	


Commit Tran  Create_stockItems




---SQL UPDATES  06/03/2020

/****Header Records *****************************************/
declare @seqno int = (select max(seqno)+1 from cr_trans),
@branchno int = (select id from BRANCHES where BCODE =  '{{z.document.location.code}}') ,

@accno int = {{z.document.supplier.linked.accountNumber}},
@transdate datetime = '{{ date.parse z.document.orderDate | date.to_string "%F" }}',
@dueDate datetime = '{{ date.parse z.document.orderDate | date.to_string "%F" }}',
@name varchar = Concat('{{z.document.supplier.linked.accountNumber}}','.','{{z.document.supplier.linked.name}}') ,
@invno varchar(20)= '{{z.document.invoiceNumber}}', 
@transtype int = 1,
@exchrate float = 1,
@subtotal float = {{value z.document.lines.subtotal}},
@taxtotal float = {{value z.document.lines.tax}},
@period_seqno int = 0

/***********************************************************/




 declare @createInvoice int = 0 , @error int = 0
 begin 
  set @createInvoice =  (select COUNT(*) from CR_TRANS where INVNO =  @invno and ACCNO = @accno)
 end 

 BEGIN 

 if  @createInvoice < 1

 BEGIN

 BEGIN TRAN Create_Invoice



  SET IDENTITY_INSERT  CR_TRANS ON
  SET @period_seqno = (SELECT MAX(SEQNO)
        from period_status ps 
        where ps.STARTDATE <=  @transdate and ps.STOPDATE >= EOMONTH(@transdate) and ps.LEDGER = 'C')

  INSERT INTO [dbo].[CR_TRANS]
           (
       [SEQNO] ,
    [BRANCHNO] ,
    [ACCNO],     
    [TRANSTYPE],
       [TRANSDATE],
    [DUEDATE],
    [INVNO],
    [EXCHRATE],
    [SUBTOTAL],
    [TAXTOTAL],
    [PERIOD_SEQNO]     
   )
    VALUES
      (
     @seqno,
     @branchno,
     @accno,
     @transtype,
     @transdate,
     ISNULL(@dueDate,@transdate),-- might need to put into a function to calcuate the default terms from the supplier card
     @invno,
     @exchrate,
     @subtotal,
     @taxtotal,
     @period_seqno
    
     )  
   SET IDENTITY_INSERT  CR_TRANS OFF

   commit tran Create_Invoice
  
  END
 
 else 
   Begin
    RAISERROR('Duplicate Invioce',16,1)
    set @error = 1
    
   end

    declare @laccno int,
  @linvno varchar(20),     
  @lhdr_seqno int,
  @lstockcode varchar(23),
  @llinked_stockcode varchar(23),  -- must be used for GL and has to be the same as stockcode otherwise issues when user posts to Gl  
  @ldescription varchar(23),
  @ltaxrate float,
  @lquantity float,
  @llinked_qty float,-- must be used for GL and has to be the same as stockcode otherwise issues when user posts to Gl
  @lunitprice float,
  @ldiscount float,
  @ldiscountamt float,
  @ldiscountpct float,
    /**Scrip Variables**/
  @rounding int,  
  @llocation int,
  @lcodeType varchar,
  @llinetax_overridden char,
  @ltaxrate_no int,
  @lline_source int ,  
  @lunitprice_inctax float,  
  @llinetotal_tax float

 BEGIN 
  {{- for line in z.document.lines}} 
 if @error <> 1 



 Begin
 

 BEGIN TRAN Create_Invoice

/** Templating Language to loop for each and set these variables*/
/***********Lines******************************************/




  
  set @laccno = @accno
  set @linvno  = @invno     
  set @lhdr_seqno  = @seqno
  set @lstockcode  = CONCAT(
  set @llinked_stockcode = '00-03720-00'  -- must be used for GL and has to be the same as stockcode otherwise issues when user posts to Gl
  
  set @ldescription = ''
  set @ltaxrate = 10
  set @lquantity = 1
  set @llinked_qty = 1 -- must be used for GL and has to be the same as stockcode otherwise issues when user posts to Gl

  set @lunitprice = 10
  set @ldiscount = 0
  set @ldiscountamt = 0
  set @ldiscountpct = 0



  /**Scrip Variables**/
     set @rounding = (Select top 1 Convert(int,fieldvalue)  FROM PROFILE_VALUES  where FIELDNAME = 'COSTPRDECIMALS')  
  set @llocation = 1 
  set @lcodeType = 'G'
  set @llinetax_overridden = 'N'
  set @ltaxrate_no = 32
  set @lline_source  = 0   
  set @lunitprice_inctax  = (Select 
             CASE
              WHEN @ltaxrate > 0 then Round(SUM(@lunitprice*(@ltaxrate/100)+ @lunitprice),@rounding)
              ELSE @lunitprice END )-- will need to add in logic for discounting rates later. 
  
  set @llinetotal_tax  = (Select      CASE
              WHEN @ltaxrate > 0 then Round(SUM(@lunitprice*@lquantity) *(@ltaxrate/100),@rounding)
              ELSE 0 END )-- will need to add in logic for discounting rates later. 










/*******************************************************/

 INSERT INTO [dbo].[CR_INVLINES](
           [ACCNO],
     [INVNO],
     [HDR_SEQNO],
     [STOCKCODE],
     [LINKED_STOCKCODE],
     [LINKED_QTY],
     [DESCRIPTION],
     [QUANTITY],
     [UNITPRICE],
     [TAXRATE],
     [DISCOUNT],
     [DISCOUNTAMT],
     [DISCOUNTPCT],
     [LOCATION],
     [CODETYPE],
     [LINETAX_OVERRIDDEN],
     [TAXRATE_NO],
     [LINE_SOURCE],
     [UNITPRICE_INCTAX],
     [LINETOTAL_TAX]
     )
     VALUES
           (@laccno,
   @linvno,     
   @lhdr_seqno,
   @lstockcode,
   @llinked_stockcode,
   @llinked_qty,
   @ldescription,
   @lquantity,
   @lunitprice,
   @ltaxrate,
   @ldiscount ,
   @ldiscountamt,
   @ldiscountpct,
   @llocation,
   @lcodeType,
   @llinetax_overridden,
   @ltaxrate_no,
   @lline_source,
   @lunitprice_inctax,
   @llinetotal_tax

  )

  Commit TRAN Create_Invoice
         
   END
     {{ end }} 
 END
END

--Changes 06/03/2020

---SQL UPDATES  06/03/2020

/****Header Records *****************************************/
declare @seqno int = (select max(seqno)+1 from cr_trans),
@branchno int =  {{z.document.location.code}},

@accno int = {{z.document.supplier.linked.accountNumber}},
@transdate datetime = '{{ date.parse z.document.orderDate | date.to_string "%F" }}',
@dueDate datetime = '{{ date.parse z.document.orderDate | date.to_string "%F" }}',
@name varchar = '{{z.document.supplier.linked.accountNumber}}'.'{{z.document.supplier.linked.name}}',
@invno varchar(20)= '{{z.document.invoiceNumber}}', 
@transtype int = 1,
@exchrate float = 1,
@subtotal float = {{value z.document.lines.subtotal}},
@taxtotal float = {{value z.document.lines.tax}},
@period_seqno int = 0

/***********************************************************/




 declare @createInvoice int = 0 , @error int = 0
 begin 
  set @createInvoice =  (select COUNT(*) from CR_TRANS where INVNO =  @invno and ACCNO = @accno)
 end 

 BEGIN 

 if  @createInvoice < 1

 BEGIN

 BEGIN TRAN Create_Invoice



  SET IDENTITY_INSERT  CR_TRANS ON
  SET @period_seqno = (SELECT MAX(SEQNO)
        from period_status ps 
        where ps.STARTDATE <=  @transdate and ps.STOPDATE >= EOMONTH(@transdate) and ps.LEDGER = 'C')

  INSERT INTO [dbo].[CR_TRANS]
           (
    [SEQNO] ,
    [BRANCHNO] ,
    [ACCNO],     
    [TRANSTYPE],
    [TRANSDATE],
    [DUEDATE],
    [INVNO],
    [EXCHRATE],
    [SUBTOTAL],
    [TAXTOTAL],
    [PERIOD_SEQNO]     
   )
    VALUES
      (
     @seqno,
     @branchno,
     @accno,
     @transtype,
     @transdate,
     ISNULL(@dueDate,@transdate),-- might need to put into a function to calcuate the default terms from the supplier card
     @invno,
     @exchrate,
     @subtotal,
     @taxtotal,
     @period_seqno
    
     )  
   SET IDENTITY_INSERT  CR_TRANS OFF

   commit tran Create_Invoice
  
  END
 
 else 
   Begin
    RAISERROR('Duplicate Invioce',16,1)
    set @error = 1
    
   end

    declare @laccno int,
  @linvno varchar(20),     
  @lhdr_seqno int,
  @lstockcode varchar(23),
  @llinked_stockcode varchar(23),  -- must be used for GL and has to be the same as stockcode otherwise issues when user posts to Gl  
  @ldescription varchar(23),
  @ltaxrate float,
  @lquantity float,
  @llinked_qty float,-- must be used for GL and has to be the same as stockcode otherwise issues when user posts to Gl
  @lunitprice float,
  @ldiscount float,
  @ldiscountamt float,
  @ldiscountpct float,
    /**Scrip Variables**/
  @rounding int,  
  @llocation int,
  @lcodeType varchar,
  @llinetax_overridden char,
  @ltaxrate_no int,
  @lline_source int ,  
  @lunitprice_inctax float,  
  @llinetotal_tax float

 BEGIN 
  {{- for line in z.document.lines}} 
 if @error <> 1 



 Begin


 BEGIN TRAN Create_Invoice

/** Templating Language to loop for each and set these variables*/
/***********Lines******************************************/  
  set @laccno = @accno
  set @linvno  = @invno     
  set @lhdr_seqno  = @seqno
  set @lstockcode  = '0{{line.location.code}}-{{line.accountCode}}'
  set @llinked_stockcode = @lstockcode  -- must be used for GL and has to be the same as stockcode otherwise issues when user posts to Gl
  
  set @ldescription = '{{line.description}}'
  set @ltaxrate = {{line.taxType.rate}}
  set @lquantity = {{line.orderQuantity}}
  set @llinked_qty = {{line.orderQuantity}} -- must be used for GL and has to be the same as stockcode otherwise issues when user posts to Gl

  set @lunitprice = {{value line.unitPrice}}
  set @ldiscount = 0
  set @ldiscountamt = 0
  set @ldiscountpct = 0

 
  
  /**Scrip Variables**/
  set @rounding = (Select top 1 Convert(int,fieldvalue)  FROM PROFILE_VALUES  where FIELDNAME = 'COSTPRDECIMALS')  
  set @llocation = 1 
  set @lcodeType = 'G'
  set @llinetax_overridden = 'N'
  set @ltaxrate_no = (select max(SEQNO) from TAX_RATES where SHORTNAME = '{{line.taxType.code}}')
  set @lline_source  = 0   --USED FOR RECEIPTING 
  set @lunitprice_inctax  = (Select 
             CASE
              WHEN @ltaxrate > 0 then Round(SUM(@lunitprice*(@ltaxrate/100)+ @lunitprice),@rounding)
              ELSE @lunitprice END )-- will need to add in logic for discounting rates later. 
  
  set @llinetotal_tax  = (Select      CASE
              WHEN @ltaxrate > 0 then Round(SUM(@lunitprice*@lquantity) *(@ltaxrate/100),@rounding)
              ELSE 0 END )-- will need to add in logic for discounting rates later. 










/*******************************************************/

 INSERT INTO [dbo].[CR_INVLINES](
           [ACCNO],
     [INVNO],
     [HDR_SEQNO],
     [STOCKCODE],
     [LINKED_STOCKCODE],
     [LINKED_QTY],
     [DESCRIPTION],
     [QUANTITY],
     [UNITPRICE],
     [TAXRATE],
     [DISCOUNT],
     [DISCOUNTAMT],
     [DISCOUNTPCT],
     [LOCATION],
     [CODETYPE],
     [LINETAX_OVERRIDDEN],
     [TAXRATE_NO],
     [LINE_SOURCE],
     [UNITPRICE_INCTAX],
     [LINETOTAL_TAX]
     )
     VALUES
           (@laccno,
   @linvno,     
   @lhdr_seqno,
   @lstockcode,
   @llinked_stockcode,
   @llinked_qty,
   @ldescription,
   @lquantity,
   @lunitprice,
   @ltaxrate,
   @ldiscount ,
   @ldiscountamt,
   @ldiscountpct,
   @llocation,
   @lcodeType,
   @llinetax_overridden,
   @ltaxrate_no,
   @lline_source,
   @lunitprice_inctax,
   @llinetotal_tax

  )

  Commit TRAN Create_Invoice
         
   END
     {{ end }} 
 END
END