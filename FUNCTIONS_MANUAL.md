# Functions Manual - FunctEngine Library

## Table of Contents

1. [Basic Functions](#basic-functions)
2. [Math Functions](#math-functions)
3. [String Functions](#string-functions)
4. [Array Functions](#array-functions)
5. [Statistics Functions](#statistics-functions)
6. [Database Functions](#database-functions)
7. [Date & Time Functions](#date--time-functions)
8. [Financial Functions](#financial-functions)
9. [Design of Experiments Functions](#design-of-experiments-functions)

---

## Basic Functions

Basic operations for general purpose programming.

### Print
Outputs text to the console.
```
Print(value)
```
**Parameters:**
- `value` - The value to print

**Example:**
```
Print("Hello World")
```

---

### Concat
Concatenates multiple values into a single string.
```
Concat(value1, value2, ...)
```
**Parameters:**
- `value1, value2, ...` - Values to concatenate

**Example:**
```
Concat("Hello", " ", "World")  // Returns: "Hello World"
```

---

### ToString
Converts a value to its string representation.
```
ToString(value)
```
**Parameters:**
- `value` - The value to convert

**Returns:** String representation

---

### Add
Adds two numbers.
```
Add(a, b)
```
**Parameters:**
- `a` - First number
- `b` - Second number

**Returns:** Sum of a and b

---

### Multiply
Multiplies two numbers.
```
Multiply(a, b)
```
**Parameters:**
- `a` - First number
- `b` - Second number

**Returns:** Product of a and b

---

### CountWords
Counts the number of words in a text.
```
CountWords(text)
```
**Parameters:**
- `text` - The text to analyze

**Returns:** Number of words

---

### GetTextStats
Displays accumulated text analysis statistics.
```
GetTextStats()
```
**Returns:** Displays statistics including total texts analyzed, words processed, and average words per text

---

## Math Functions

Mathematical operations and constants.

### Subtract
Subtracts two numbers.
```
Subtract(a, b)
```
**Returns:** a - b

---

### Divide
Divides two numbers.
```
Divide(a, b)
```
**Returns:** a / b

---

### Power
Raises a number to a power.
```
Power(base, exponent)
```
**Returns:** base^exponent

---

### Sqrt
Calculates the square root.
```
Sqrt(value)
```
**Returns:** Square root of value

---

### Abs
Returns the absolute value.
```
Abs(value)
```
**Returns:** |value|

---

### Floor
Rounds down to the nearest integer.
```
Floor(value)
```
**Returns:** Largest integer ≤ value

---

### Ceil
Rounds up to the nearest integer.
```
Ceil(value)
```
**Returns:** Smallest integer ≥ value

---

### Round
Rounds to the specified number of decimals.
```
Round(value, decimals)
```
**Parameters:**
- `value` - Number to round
- `decimals` - Number of decimal places (optional, default: 0)

---

### Trigonometric Functions

```
Sin(angle)    // Sine (angle in radians)
Cos(angle)    // Cosine (angle in radians)
Tan(angle)    // Tangent (angle in radians)
```

---

### Logarithmic Functions

```
Log(value)    // Natural logarithm (base e)
Log10(value)  // Logarithm base 10
Exp(value)    // e^value
```

---

### Min / Max
Returns the minimum or maximum value.
```
Min(value1, value2, ...)
Max(value1, value2, ...)
```

---

### Random
Generates random numbers.
```
Random()              // Returns 0.0 to 1.0
Random(max)           // Returns 0 to max-1
Random(min, max)      // Returns min to max-1
```

---

### Constants

```
PI()    // Returns π (3.14159...)
E()     // Returns e (2.71828...)
```

---

## String Functions

String manipulation and analysis.

### GetLength
Returns the length of a string.
```
GetLength(str)
```

---

### Substring
Extracts a portion of a string.
```
Substring(str, start)
Substring(str, start, length)
```
**Parameters:**
- `str` - Source string
- `start` - Starting position
- `length` - Number of characters (optional)

---

### IndexOf
Finds the position of a substring.
```
IndexOf(str, search)
```
**Returns:** Position of search string, or -1 if not found

---

### ToUpper / ToLower
Converts string case.
```
ToUpper(str)    // Converts to uppercase
ToLower(str)    // Converts to lowercase
```

---

### Trim
Removes whitespace from both ends.
```
Trim(str)
```

---

### Replace
Replaces occurrences of a substring.
```
Replace(str, oldValue, newValue)
```

---

### Split
Splits a string into an array.
```
Split(str, separator)
```
**Parameters:**
- `str` - String to split
- `separator` - Delimiter (optional, default: space)

**Returns:** Array of substrings

---

### StartsWith / EndsWith / Contains
String search operations.
```
StartsWith(str, prefix)    // Returns true if str starts with prefix
EndsWith(str, suffix)      // Returns true if str ends with suffix
Contains(str, search)      // Returns true if str contains search
```

---

### PadLeft / PadRight
Pads a string to a specified length.
```
PadLeft(str, totalWidth, paddingChar)
PadRight(str, totalWidth, paddingChar)
```
**Parameters:**
- `str` - String to pad
- `totalWidth` - Desired total width
- `paddingChar` - Character to use for padding (optional, default: space)

---

## Array Functions

Array creation and manipulation.

### CreateArray
Creates a new array.
```
CreateArray(item1, item2, ...)
```
**Returns:** New array containing the specified items

---

### GetLength
Returns the number of elements in an array.
```
GetLength(array)
```

---

### GetElement
Retrieves an element at the specified index.
```
GetElement(array, index)
```

---

### SetElement
Sets the value at the specified index.
```
SetElement(array, index, value)
```

---

### Push
Adds an element to the end of an array.
```
Push(array, value)
```
**Returns:** New length of the array

---

### Pop
Removes and returns the last element.
```
Pop(array)
```
**Returns:** The removed element

---

### Slice
Extracts a portion of an array.
```
Slice(array, start, count)
```
**Parameters:**
- `array` - Source array
- `start` - Starting index
- `count` - Number of elements to extract

**Returns:** New array with extracted elements

---

### Join
Joins array elements into a string.
```
Join(array, separator)
```
**Parameters:**
- `array` - Array to join
- `separator` - String to use between elements (optional, default: ",")

---

### Sort
Sorts array elements.
```
Sort(array)
```
**Returns:** New sorted array (numeric sort)

---

### Reverse
Reverses the order of elements.
```
Reverse(array)
```
**Returns:** New reversed array

---

## Statistics Functions

Statistical analysis and calculations.

### Mean
Calculates the arithmetic mean.
```
Mean(value1, value2, ...)
Mean(array)
```

---

### Median
Calculates the median value.
```
Median(value1, value2, ...)
Median(array)
```

---

### Mode
Finds the most frequent value.
```
Mode(value1, value2, ...)
Mode(array)
```

---

### Range
Calculates the range (max - min).
```
Range(value1, value2, ...)
Range(array)
```

---

### Variance
Calculates the population variance.
```
Variance(value1, value2, ...)
Variance(array)
```

---

### StandardDeviation
Calculates the population standard deviation.
```
StandardDeviation(value1, value2, ...)
StandardDeviation(array)
```

---

### Sum
Calculates the sum of values.
```
Sum(value1, value2, ...)
Sum(array)
```

---

### Count
Counts the number of values.
```
Count(value1, value2, ...)
Count(array)
```

---

### Percentile
Calculates the specified percentile.
```
Percentile(value1, value2, ..., percentile)
Percentile(array, percentile)
```
**Parameters:**
- `percentile` - Value between 0 and 100

---

### Quartile
Calculates the specified quartile.
```
Quartile(value1, value2, ..., quartile)
Quartile(array, quartile)
```
**Parameters:**
- `quartile` - 1, 2, or 3 (Q1, Q2, Q3)

---

### Correlation
Calculates the Pearson correlation coefficient.
```
Correlation(array1, array2)
```
**Returns:** Value between -1 and 1

---

### ZScore
Calculates the z-score for a value.
```
ZScore(value, array)
```

---

### CreateHistogram
Creates a histogram from data.
```
CreateHistogram(dataArray, numberOfBins)
```
**Returns:** List of bins with frequencies

---

### PrintHistogram
Displays a histogram in text format.
```
PrintHistogram(histogram, maxWidth)
```
**Parameters:**
- `histogram` - Histogram data from CreateHistogram
- `maxWidth` - Maximum bar width (optional, default: 50)

---

## Database Functions

Database connectivity and operations.

### ConnectPostgres
Establishes a PostgreSQL connection.
```
ConnectPostgres(connectionName, connectionString)
```
**Returns:** true if successful

---

### ConnectSqlServer
Establishes a SQL Server connection.
```
ConnectSqlServer(connectionName, connectionString)
```
**Returns:** true if successful

---

### DisconnectDB
Closes a database connection.
```
DisconnectDB(connectionName)
```

---

### ExecuteQuery
Executes a query and returns results.
```
ExecuteQuery(connectionName, query, param1, param2, ...)
```
**Returns:** Array of result rows

---

### ExecuteNonQuery
Executes a command (INSERT, UPDATE, DELETE).
```
ExecuteNonQuery(connectionName, query, param1, param2, ...)
```
**Returns:** Number of affected rows

---

### ExecuteScalar
Executes a query and returns a single value.
```
ExecuteScalar(connectionName, query, param1, param2, ...)
```
**Returns:** Single value result

---

### GetRowValue
Retrieves a column value from a result row.
```
GetRowValue(row, columnName)
```

---

### GetRowKeys
Gets all column names from a result row.
```
GetRowKeys(row)
```
**Returns:** Array of column names

---

### Transaction Functions

```
BeginTransaction(connectionName)     // Starts a transaction
CommitTransaction(connectionName)    // Commits a transaction
RollbackTransaction(connectionName)  // Rolls back a transaction
```

---

## Date & Time Functions

Date and time manipulation functions.

### Date
Returns the current date or formats a date.
```
Date(format)
```
**Parameters:**
- `format` - Date format string (optional, default: "yyyy-MM-dd")

**Example:**
```
Date("yyyy-MM-dd")           // 2025-09-29
Date("dd/MM/yyyy HH:mm")     // 29/09/2025 14:30
```

---

### Datediff
Calculates the difference in days between two dates.
```
Datediff(startDate, endDate)
```
**Returns:** Number of days

---

### Datevalue
Converts a text string to a date.
```
Datevalue(dateText)
```

---

### Day / Month / Year
Extracts components from a date.
```
Day(date)      // Returns 1-31
Month(date)    // Returns 1-12
Year(date)     // Returns 4-digit year
```

---

### Hour / Minute / Second
Extracts time components.
```
Hour(dateTime)     // Returns 0-23
Minute(dateTime)   // Returns 0-59
Second(dateTime)   // Returns 0-59
```

---

### Now
Returns the current date and time.
```
Now(format)
```
**Parameters:**
- `format` - DateTime format string (optional, default: "yyyy-MM-dd HH:mm:ss")

---

### Quarter
Returns the quarter of the year.
```
Quarter(date)
```
**Returns:** 1, 2, 3, or 4

---

### Time
Creates a time value from components.
```
Time(hours, minutes, seconds)
```

---

### Timevalue
Converts a text string to a time.
```
Timevalue(timeText)
```

---

### Today
Returns today's date (without time).
```
Today()
```

---

### UtcNow
Returns the current UTC date and time.
```
UtcNow(format)
```

---

### WeekDay
Returns the day of the week.
```
WeekDay(date)
```
**Returns:** 0 (Sunday) to 6 (Saturday)

---

### WeekNum
Returns the week number of the year.
```
WeekNum(date)
```

---

### YearFrac
Returns the fraction of the year between two dates.
```
YearFrac(startDate, endDate)
```

---

## Financial Functions

Financial calculations and analysis.

### FV - Future Value
Calculates the future value of an investment.
```
Fv(rate, nper, pmt, pv, type)
```
**Parameters:**
- `rate` - Interest rate per period
- `nper` - Number of periods
- `pmt` - Payment per period
- `pv` - Present value (optional, default: 0)
- `type` - 0 = end of period, 1 = beginning (optional, default: 0)

---

### PV - Present Value
Calculates the present value.
```
Pv(rate, nper, pmt, fv, type)
```

---

### PMT - Payment
Calculates the payment for a loan.
```
Pmt(rate, nper, pv, fv, type)
```

---

### NPER - Number of Periods
Calculates the number of periods for an investment.
```
Nper(rate, pmt, pv, fv, type)
```

---

### RATE - Interest Rate
Calculates the interest rate per period.
```
Rate(nper, pmt, pv, fv, type, guess)
```
**Parameters:**
- `guess` - Initial guess for rate (optional, default: 0.1)

---

### IPMT - Interest Payment
Calculates the interest payment for a period.
```
Ipmt(rate, period, nper, pv, fv, type)
```

---

### PPMT - Principal Payment
Calculates the principal payment for a period.
```
Ppmt(rate, period, nper, pv, fv, type)
```

---

### CUMIPMT - Cumulative Interest
Calculates cumulative interest paid between periods.
```
Cumipmt(rate, nper, pv, startPeriod, endPeriod, type)
```

---

### CUMPRINC - Cumulative Principal
Calculates cumulative principal paid between periods.
```
Cumprinc(rate, nper, pv, startPeriod, endPeriod, type)
```

---

### Depreciation Functions

```
Sln(cost, salvage, life)              // Straight-line depreciation
Syd(cost, salvage, life, period)      // Sum-of-years digits
Db(cost, salvage, life, period, month)  // Declining balance
Ddb(cost, salvage, life, period, factor) // Double-declining balance
```

---

### Interest Rate Conversion

```
Effect(nominalRate, npery)    // Effective annual rate
Nominal(effectRate, npery)    // Nominal annual rate
```

---

### XNPV - Net Present Value
Calculates NPV for irregular cash flows.
```
Xnpv(rate, values[], dates[])
```

---

### XIRR - Internal Rate of Return
Calculates IRR for irregular cash flows.
```
Xirr(values[], dates[], guess)
```

---

### RRI - Compound Growth Rate
Calculates the equivalent interest rate for growth.
```
Rri(nper, pv, fv)
```

---

### PDURATION - Period Duration
Calculates periods required to reach a value.
```
Pduration(rate, pv, fv)
```

---

### ISPMT - Interest Payment (Linear)
Calculates interest for even principal payments.
```
Ispmt(rate, period, nper, pv)
```

---

### Dollar Conversion

```
Dollarde(fractionalDollar, fraction)  // Fraction to decimal
Dollarfr(decimalDollar, fraction)     // Decimal to fraction
```

---

### Securities Functions

```
Disc(settlement, maturity, pr, redemption, basis)        // Discount rate
Intrate(settlement, maturity, investment, redemption, basis)  // Interest rate
Received(settlement, maturity, investment, discount, basis)   // Amount at maturity
Pricedisc(settlement, maturity, discount, redemption, basis)  // Price of discounted security
```

---

### Treasury Bill Functions

```
Tbillprice(settlement, maturity, discount)  // T-Bill price
TbillYield(settlement, maturity, pr)        // T-Bill yield
Tbilleq(settlement, maturity, discount)     // Bond-equivalent yield
```

---

## Design of Experiments Functions

Statistical analysis for experimental design.

### LinearRegression
Performs simple linear regression.
```
LinearRegression(xValues[], yValues[])
```
**Returns:** (slope, intercept, rSquared)

---

### MultipleLinearRegression
Performs multiple linear regression.
```
MultipleLinearRegression(xMatrix[][], yValues[])
```
**Returns:** Array of coefficients [intercept, coef1, coef2, ...]

---

### OneWayAnova
Performs one-way analysis of variance.
```
OneWayAnova(groups[][])
```
**Parameters:**
- `groups` - Array of arrays, each containing data for one group

**Returns:** (fStatistic, pValue, ssWithin, ssBetween)

---

### TwoWayAnova
Performs two-way analysis of variance.
```
TwoWayAnova(data[][], factorA[], factorB[])
```
**Returns:** (fA, fB, fInteraction, ssTotal)

---

### Manova
Performs multivariate analysis of variance.
```
Manova(groups[][][])
```
**Returns:** (wilksLambda, pillaiTrace)

---

### Glm
Fits a generalized linear model.
```
Glm(xMatrix[][], yValues[], family)
```
**Parameters:**
- `family` - "gaussian", "binomial", etc. (optional, default: "gaussian")

**Returns:** Array of coefficients

---

### GageRR
Performs Gage R&R analysis.
```
GageRR(measurements[][][])
```
**Parameters:**
- `measurements` - 3D array [parts][operators][replicates]

**Returns:** (repeatability, reproducibility, gageRR, partVariation)

---

### Statistical Helper Functions

```
Mean(values[])                        // Arithmetic mean
Variance(values[])                    // Variance
StandardDeviation(values[])           // Standard deviation
Correlation(xValues[], yValues[])     // Pearson correlation
```

---

## Usage Examples

### Financial Example
```javascript
// Calculate monthly payment for a $200,000 loan at 5% annual rate for 30 years
rate = 0.05 / 12        // Monthly rate
nper = 30 * 12          // Total months
pv = 200000             // Loan amount
payment = Pmt(rate, nper, pv)
Print(payment)          // Returns: -1073.64
```

### Statistics Example
```javascript
// Analyze sales data
sales = CreateArray(100, 150, 120, 180, 145, 160, 175)
avgSales = Mean(sales)
stdDev = StandardDeviation(sales)
Print(Concat("Average: ", avgSales, ", Std Dev: ", stdDev))
```

### Date Example
```javascript
// Calculate days until a deadline
today = Today()
deadline = Datevalue("2025-12-31")
daysLeft = Datediff(today, deadline)
Print(Concat("Days remaining: ", daysLeft))
```

### Database Example
```javascript
// Query database
ConnectPostgres("mydb", "Host=localhost;Database=sales;Username=user;Password=pass")
results = ExecuteQuery("mydb", "SELECT * FROM products WHERE price > @p1", 100)
DisconnectDB("mydb")
```

### Regression Example
```javascript
// Perform linear regression
xValues = CreateArray(1, 2, 3, 4, 5)
yValues = CreateArray(2, 4, 5, 4, 5)
result = LinearRegression(xValues, yValues)
Print(Concat("Slope: ", result.slope, ", R²: ", result.rSquared))
```

---

## Notes

- All angle parameters for trigonometric functions are in radians
- Array indices are zero-based (first element is at index 0)
- Date format strings follow .NET DateTime format specifiers
- Financial functions follow Excel conventions for parameter order
- Statistical functions can accept either individual values or arrays

---

**Version:** 1.0
**Last Updated:** September 2025