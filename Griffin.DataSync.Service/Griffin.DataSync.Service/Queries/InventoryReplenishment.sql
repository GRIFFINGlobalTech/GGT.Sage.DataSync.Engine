--To implemented

/*select d.itemcode, d.qty,d.QtyOnHand ,d.ItemDesc from
( select  C.ItemCode, sum(c.qty) as Qty, sum(c.QtyOnHand) as QtyOnHand, C.ItemDesc from  
( select MB.ItemCode, sum(MB.QuantityOnHand) as Qty, sum(MB.QuantityOnHand) as QtyOnHand, ci.ItemCodeDesc as ItemDesc from MB_BinItem as MB
join MB_BinLocation as BL on mb.BinLocation = bl.BinLocation join ci_Item as CI on mb.ItemCode = ci.ItemCode 
where    bl.active = 'y' and (((MB.binlocation like '[0-9]%A%[0-9]' or mb.binlocation= '01210WALL') and  MB.WarehouseCode = '001') or (MB.binlocation like '%A%' and  MB.WarehouseCode = '003'))
group by mb.ItemCode, ci.ItemCodeDesc
union all select S.ItemCode,-sum(quantityordered) as Qty, 0 as QtyOnHand,ci.ItemCodeDesc as ItemDesc 
from SO_SalesOrderDetail as S join SO_SalesOrderHeader as SH on S.SalesOrderNo = SH.SalesOrderNo 
join ci_Item as CI on S.ItemCode = ci.ItemCode where YEAR(SH.OrderDate) = year(getdate()) 
and sh.[UDF_SHIP_BY_DATE] < dateadd(day, 14, getdate()) and sh.[UDF_SHIP_BY_DATE] >= dateadd(day, -1, getdate())
and sh.OrderType <> 'q' group by S.ItemCode,  ci.ItemCodeDesc) C where c.ItemCode not like '%/%'  group by c.ItemCode, c.ItemDesc ) D where d.Qty < 0 and d.QtyOnHand > 0  order by d.ItemCode*/

;WITH AInventory AS
(
    SELECT
        MB.ItemCode,
        SUM(MB.QuantityOnHand) AS Qty,
        SUM(MB.QuantityOnHand) AS QtyOnHand,
        CI.ItemCodeDesc AS ItemDesc
    FROM MB_BinItem MB
    INNER JOIN MB_BinLocation BL
        ON MB.BinLocation = BL.BinLocation
    INNER JOIN CI_Item CI
        ON MB.ItemCode = CI.ItemCode
    WHERE
        BL.Active = 'Y'
        AND
        (
            (
                (MB.BinLocation LIKE '[0-9]%A%[0-9]'
                 OR MB.BinLocation = '01210WALL')
                AND MB.WarehouseCode = '001'
            )
            OR
            (
                MB.BinLocation LIKE '%A%'
                AND MB.WarehouseCode = '003'
            )
        )
    GROUP BY
        MB.ItemCode,
        CI.ItemCodeDesc
),
SalesDemand AS
(
    SELECT
        S.ItemCode,
        -SUM(S.QuantityOrdered) AS Qty,
        CI.ItemCodeDesc AS ItemDesc
    FROM SO_SalesOrderDetail S
    INNER JOIN SO_SalesOrderHeader SH
        ON S.SalesOrderNo = SH.SalesOrderNo
    INNER JOIN CI_Item CI
        ON S.ItemCode = CI.ItemCode
    WHERE
        YEAR(SH.OrderDate) = YEAR(GETDATE())
        AND SH.UDF_SHIP_BY_DATE >= GETDATE()
        AND SH.UDF_SHIP_BY_DATE < DATEADD(DAY,14,GETDATE())
        AND SH.OrderType <> 'Q'
    GROUP BY
        S.ItemCode,
        CI.ItemCodeDesc
),
AvailableQty AS
(
    SELECT
        MB.ItemCode,
        SUM(MB.QuantityOnHand) AS ItemQtyAvailable
    FROM MB_BinItem MB
    INNER JOIN MB_BinLocation BL
        ON MB.BinLocation = BL.BinLocation
    WHERE
        MB.BinLocation LIKE '[0-9]%B%[0-9]'
        OR MB.BinLocation LIKE '[0-9]%C%[0-9]'
    GROUP BY
        MB.ItemCode
),
Combined AS
(
    SELECT
        ItemCode,
        Qty,
        QtyOnHand,
        ItemDesc
    FROM AInventory

    UNION ALL

    SELECT
        ItemCode,
        Qty,
        0,
        ItemDesc
    FROM SalesDemand
)

SELECT
    C.ItemCode,
    MAX(C.ItemDesc) AS ItemDesc,
    SUM(C.Qty) AS ItemQty,
    SUM(C.QtyOnHand) AS QtyOnHand,
    ISNULL(A.ItemQtyAvailable,0) AS ItemQtyAvailable
FROM Combined C
LEFT JOIN AvailableQty A
    ON C.ItemCode = A.ItemCode
WHERE
    C.ItemCode NOT LIKE '%/%'
GROUP BY
    C.ItemCode,
    A.ItemQtyAvailable
HAVING
    SUM(C.Qty) < 0
    AND SUM(C.QtyOnHand) > 0
ORDER BY
    C.ItemCode;