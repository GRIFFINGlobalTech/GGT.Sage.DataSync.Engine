SELECT TOP 10
    S.ItemCode,
    SUM(S.QuantityOrdered) AS Qty,
FROM
    SO_SalesOrderDetail S,
    SO_SalesOrderHeader SH,
    CI_Item CI
WHERE
    S.SalesOrderNo = SH.SalesOrderNo
AND S.ItemCode = CI.ItemCode
