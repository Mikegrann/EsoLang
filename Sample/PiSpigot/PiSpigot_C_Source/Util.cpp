#include "Util.hpp"

// Negates the boolean value of x
// input x is true for >0
// output is true for x<=0, false otherwise
// Equivalent to !(x > 0)
int Not(int x) {
	return (x > 0) ? f : t;
}

// Equivalent to (x == y)
int Eq(int x, int y) {
	int diff = x + (-y);

	return (diff > 0) ? f : ((-diff > 0) ? f : t);
}

// Equivalent to (x > y)
int Gt(int x, int y) {
	int diff = x + (-y);

	return (diff > 0) ? t : f;
}

// Equivalent to (x < y)
int Lt(int x, int y) {
	int diff = x + (-y);

	return (diff > 0) ? f : (Eq(diff, zero) ? f : t);
}

// Equivalent to (x * y)
// Uses the identity x*y == (x-1)*y + y
// Checks for negatives to ensure recursion ends
int Mult(int x, int y) {
	if (-x > 0) {
		x = -x;
		y = -y;
	}
	else { }

	int diff = x + (-one);

	return (x > 0) ? (Mult(diff, y) + y) : x;
}

// Equivalent to (x / y) for x,y>0
// Uses the identity x/y == (x-y)/y + 1
// Requires x,y>0 so does not check for negatives
// Fails for denominator 0 (no error checking)
int DivPos(int x, int y) {
	int diff = x + (-y);

	return (Gt(diff, -one)) ? (DivPos(diff, y) + one) : zero; 
}

// Equivalent to (x / y)
// Builds on DivPos but adds sign checks to support negatives
// Fails for denominator 0
int Div(int x, int y) {
	int sign;

	if (-x > 0) {
		sign = -one;
		x = -x;
	}
	else { 
		sign = one;
	}
	if (-y > 0) {
		sign = Mult(sign, -one);
		y = -y;
	}
	else { }

	int quotient = DivPos(x, y);
	
	return Mult(sign, quotient);
}

// Equivalent to (x % y) in C (technically remainder not mod)
// Uses the identity x%y == x - (x/y)*y
int Mod(int x, int y) {
	int quotient = Div(x, y);

	return x + (-Mult(quotient, y));
}