# ���������������� ������ ���������

���������������� ����������� ����� �������� XML �����, � ������� ����������� ����� � ���������.
�� ����������� ��������� �������� ���������� ������ ���������.
������ �������� �����������, ���������������, ���� �� ������, � ������� �� ���������.
��������� � ��������� ������ ��������� ��� �������� �������� ��� ������ XPath.
��� ��������� ���������� �� ������, ��� ��������� ������������ ������ ���������� ��������.
��������� ���������� ��� ������ ���������� �������, ���� �������������� ������ �� ����� SQL-injection.

> ��� ��������� ����������� ����� ��� ���� ������, �� �������� ��� �������� ������������ ��������� ����� ������.

## �������

������� ���������� �����:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<!DOCTYPE DataSource SYSTEM "BindingDoctype.dtd">
<ds:DataSource xmlns:ds="http://ntc.rosneft.ru/DBD/DocumentAggegator/Structure"
               xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
               xmlns:xsd="http://www.w3.org/2001/XMLSchema"
	           documentKind="Document">
    <!-- ����� ������������� ����� -->
</ds:DataSource>
```
��������� �� ����� ����� ������ ����� ��������� ������ ����, ���������� �� HTTP-������� (�������� `ID`).

### Query

������ ��� ������.
������������ ��� ������������� ����� ������ ���������� � ����,
������ ������� �������� ����� ��������, � ��������� - ������ ���� ������.
```xml
<ds:Query>
select 1 id, dummy
from dual
</ds:Query>
```
���� ���� ����������� �������� ���������:
```xml
<document template="file.docx">
	<id>1</id>
    <dummy>X</dummy>
</document>
```

������ ����� ������������ ��� ������ ��� �������� ��������� � ��������� ������.
```xml
<ds:Query arguments="id">
select (2 + :0) sum 
from dual
</ds:Query>
```
� ����� ����������� ��������� � ���������� �������, ������ ����������:
```xml
<document template="file.docx">
	<id>1</id>
    <dummy>X</dummy>
    <sum>3</sum>
</document>
```

### Collection

��������� ��������� ����� ��������� ����� ������ ��������.
```xml
<ds:Collection name="entry" itemIndexColumn="num" valColumn="dummy">
WITH cte(num) AS (SELECT 1 num FROM DUAL
                  UNION ALL
                  SELECT num+1 FROM cte WHERE num < 3)
SELECT * FROM cte, dual
</ds:Collection>
```
������ ������ ������ ���, ��������� � �������� `name`.
```xml
<document template="file.docx">
	<entry key="1">X</entry>
	<entry key="2">X</entry>
	<entry key="3">X</entry>
</document>
```
��� ���������� ������� ��������, ��� � ������ ������������ ���� `num` �� ��������� `3`.
����� ����, �������� � ������� ������, � ������ ��������� ����� ����� ������ �� ���������� ��� ����/������.
```xml
<ds:Collection name="entry" arguments="num" itemIndexColumn="num">
WITH cte(num) AS (SELECT 1 num FROM DUAL
                  UNION ALL
                  SELECT num+1 FROM cte WHERE num < :0)
SELECT num, num-1 sub, dummy FROM cte, dual
</ds:Collection>
```
```xml
<document template="file.docx">
    <num>3</num>
	<entry key="1">
        <sub>0</sub>
        <dummy>X</dummy>
    </entry>
	<entry key="2">
        <sub>1</sub>
        <dummy>X</dummy>
    </entry>
	<entry key="3">
        <sub>2</sub>
        <dummy>X</dummy>
    </entry>
</document>
```

### Table

������� - ��� �� �� ���������, ������ �������� ����� ���������.
������ ������ ���������� ������� ��� �������.

```xml
<ds:Table name="line">
    <ds:For></ds:For>
    <ds:Get></ds:Get>
</ds:Table>
```
```xml
<document template="file.docx">
</document>
```
