# Document aggregator

������ ����������� Web-API ���������� ��� ������������� ����������������.

����������:
- ������ �� ������������ ��������������\
	������ ������ ������ ���� ����� ����������� � ������������ ������� �������� � ��������� ������. ��� ����� ������ ��� ������� Web-API � ����������� ASP.NET.
- �������������\
	��� ��� ������ �����, ������������� � �����������������, ������������ ����������� ������ ����������, ����������� �������� � ����� ����� � ����� ���������� ����������.\
	����� ����, ������������ ��� ������� ������� ������� "[Clean Architecture](https://fullstackmark.com/post/18/building-aspnet-core-web-apis-with-clean-architecture)", ��������� ������� ����������� ����� ����� ��������, ���������� ������������� � ������������� ����.

---
� ��������� ������� � ������� ���� ����� "������" ��� `request` �������� ������ `claim` ��� ��������� �������� � "�������� ������".

## ������������� ������� - ��������

### ������-�������

- Claim - ������
- Insert - �������

### ������ �������������

��������� ������
- ClaimRequest - ������ �� ������
- ClaimResponse - ����� �� ����������
- ClaimInteractor - ����� ��������� ������

��������� ���������
- FormRequest - ������ �� ���������
- FormResponse - ����� �� ���������
- FormInteractor - ����� ��������� �����

��������� �������
- InsertRequest - ������ �� �������
- InsertResponse - ����� �� �������
- ParseInsertInteractor - ����� ��������� �������

### ����������

��������� ������������
- ILogger - .

������� ������
- IClaimRepository - ����������� �������� `Claim`
- IMixedFieldRepository - ����������� `string` ��� ��������� �� ����� ���� `Insert`
- IEditorService - ������ ��������� ���������

## ������� ���������� - ������ �������������
|||
|-|-|
|���������|������� ����������|
|��������|������ ���� ������ ���������� �� ������� � ��������� � ������� ����������, ����������� ��� ��������� ����������. ����� ��������� ��������������� �������� �� ������.|
|�������� ����������� ����|������ ���� ������|
|��������������� �������|<ul><li>������ �������</li><li>������������ ���� ������ ���������</li></ul>|
|���������������� �������|������ ����� �� ���������� ������|
|�������� �������� ��������|<ol><li>������ �������� HTTP ������ � ������� � ������.</li><li>������� ��������� � ��������� ������ �� ���� ������.</li><li>������� ������������ ���� ������ � ����������� ������� � ��������� ������.</li><li>������� ���������� ����� � ���������� ����� ��� ������ �� ����.</li></ol>|
|����������|<ul><li>(2a) ��� ��������� ������ �� ������.</li><ol><li>������� ���������� ����� � �����, ������� ������ � ��������������� �������.</li></ol><li>(3a) ������� ���� � ������������� ���������.</li><ol><li>������� ���������� ����� � �����, ������� ������ � ��������������� �������.</li></ol></ul>|

����������� �����:
|���� ������ (��������)|���� ������� (����������)|���������|
|-|-|-|
|�����������|�����������|���������.|
|�����������|������������|��������� ������������, �� ���� ��������� ������� ������.<br/>������� ��������������.|
|������������|�����������|���������.<br/>*�������� ������ � ��������� ���.*|
|������������|������������|���������.|

## ������� ��������� �����������

TODO: OracleDatabase

TODO: WordService

## ������� ������������ ���������� ����������

### ������ ������ ASP .NET Core

�������� ��� ������ � ��������� ���������:
```
Content-Type: application/problem+json; charset=utf-8
```

���������� ������ ������� �� ������.
