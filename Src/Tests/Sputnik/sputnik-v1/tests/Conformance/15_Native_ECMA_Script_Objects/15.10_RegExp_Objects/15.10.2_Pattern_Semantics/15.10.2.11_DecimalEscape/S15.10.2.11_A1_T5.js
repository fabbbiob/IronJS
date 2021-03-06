// Copyright 2009 the Sputnik authors.  All rights reserved.
// This code is governed by the BSD license found in the LICENSE file.

/**
 * @name: S15.10.2.11_A1_T5;
 * @section: 15.10.2.11, 15.10.2.9;
 * @assertion: DecimalEscape :: DecimalIntegerLiteral [lookahead not in DecimalDigit];
 * @description: DecimalIntegerLiteral is not 0;
*/

var arr = /\1(A)/.exec("AA");

//CHECK#1
if ((arr === null) || (arr[0] !== "A")) {
  $ERROR('#1: var arr = (/\\1(A)/.exec("AA")); arr[0] === "A". Actual. ' + (arr && arr[0]));
}

//CHECK#2
if ((arr === null) || (arr[1] !== "A")) {
  $ERROR('#2: var arr = (/\\1(A)/.exec("AA")); arr[1] === "A". Actual. ' + (arr && arr[1]));
}    
