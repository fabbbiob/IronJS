// Copyright 2009 the Sputnik authors.  All rights reserved.
// This code is governed by the BSD license found in the LICENSE file.

/**
* @name: S15.3.4.4_A5_T4;
* @section: 15.3.4.4;
* @assertion: If thisArg is not null(defined) the called function is passed ToObject(thisArg) as the this value;
* @description: thisArg is function variable that return this;
*/

f = function(){this.touched= true; return this;};

retobj = f.call(obj);

//CHECK#1
if (typeof obj !== "undefined") {
  $ERROR('#1: If thisArg is not null(defined) the called function is passed ToObject(thisArg) as the this value');
}

//CHECK#2
if (!(retobj["touched"])) {
  $ERROR('#2: If thisArg is not null(defined) the called function is passed ToObject(thisArg) as the this value');
}

var obj;
