# Конфигурирование модели документа

Конфигурирование реализуется через создание XML файла, в котором перечислены блоки с запросами.
Из результатов указанных запросов собирается модель документа.
Модель строится динамически, последовательно, блок за блоком, в порядке их появления.
Указанные в атрибутах блоков аргументы для запросов задаются при помощи XPath.
Эти аргументы выбираются из модели, что позволяет использовать данные предыдущих запросов.
Аргументы передаются при помощи параметров запроса, этим обеспечивается защита от атаки SQL-injection.

> Для упрощения создавалась схема для этих файлов, но временно для простоты используются описатели типов данных.

## Примеры

Базовое содержимое файла:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<!DOCTYPE DataSource SYSTEM "BindingDoctype.dtd">
<ds:DataSource xmlns:ds="http://ntc.rosneft.ru/DBD/DocumentAggegator/Structure"
               xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
               xmlns:xsd="http://www.w3.org/2001/XMLSchema"
	           documentKind="Document">
    <!-- Здесь перечисляются блоки -->
</ds:DataSource>
```
Созданная по этому файлу модель будет содержать только поля, полученные из HTTP-запроса (например `ID`).

### Query

Первый вид блоков.
Используется для интерпретации одной строки результата в поля,
именем которых являются имена столбцов, а значением - данные этой строки.
```xml
<ds:Query>
select 1 id, dummy
from dual
</ds:Query>
```
Блок выше сгенерирует подобную структуру:
```xml
<document template="file.docx">
	<id>1</id>
    <dummy>X</dummy>
</document>
```

Следом можно использовать эту модель для передачи аргумента в следующий запрос.
```xml
<ds:Query arguments="id">
select (2 + :0) sum 
from dual
</ds:Query>
```
В итоге подстановки параметра и выполнения запроса, модель дополнится:
```xml
<document template="file.docx">
	<id>1</id>
    <dummy>X</dummy>
    <sum>3</sum>
</document>
```

### Collection

Коллекции позволяют разом загружать целый список объектов.
```xml
<ds:Collection name="entry" itemIndexColumn="num" valColumn="dummy">
WITH cte(num) AS (SELECT 1 num FROM DUAL
                  UNION ALL
                  SELECT num+1 FROM cte WHERE num < 3)
SELECT * FROM cte, dual
</ds:Collection>
```
Каждая строка примет имя, указанное в атрибуте `name`.
```xml
<document template="file.docx">
	<entry key="1">X</entry>
	<entry key="2">X</entry>
	<entry key="3">X</entry>
</document>
```
Для следующего примера допустим, что в модели присутствует поле `num` со значением `3`.
Кроме того, столбцов у запроса больше, а значит значением будут любые данные не помеченные как ключ/индекс.
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

Таблица - это та же коллекция, только заданная двумя запросами.
Первый запрос генерирует выборку для второго.

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
