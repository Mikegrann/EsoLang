#include <iostream>
#include "Util.hpp"

// Set of functions for checking Util's implementations of the EsoLang arith/bool operations

// Simple exit-on-failure test function
void TestCheck(int actual, int expected) {
	if (actual != expected) {
		std::cerr << "TEST FAILED!" << std::endl <<
			"Expected: " << expected << std::endl <<
			"Actual: " << actual << std::endl;

		exit(1);
	}
}

void TestMult() {
	TestCheck(Mult(0, 0),	0 * 0);

	TestCheck(Mult(0, 1),	0 * 1);
	TestCheck(Mult(0, -1),	0 * -1);
	TestCheck(Mult(1, 0),	1 * 0);
	TestCheck(Mult(-1, 0), -1 * 0);
	
	TestCheck(Mult(1, 5),	1 * 5);
	TestCheck(Mult(1, -5),	1 * -5);
	TestCheck(Mult(-1, 5), -1 * 5);
	TestCheck(Mult(-1, -5),-1 * -5);
	
	TestCheck(Mult(2, 4),	2 * 4);
	TestCheck(Mult(4, 2),	4 * 2);
	
	TestCheck(Mult(-5, -3),-5 * -3);	
	TestCheck(Mult(-3, -5),-3 * -5);
	
	TestCheck(Mult(-9, 4), -9 * 4);
	TestCheck(Mult(4, -9),	4 * -9);
	TestCheck(Mult(-4, 9), -4 * 9);
	TestCheck(Mult(9, -4),	9 * -4);
	
	TestCheck(Mult(-3, 3), -3 * 3);
	TestCheck(Mult(3, -3),  3 * -3);
	TestCheck(Mult(-3, -3),-3 * -3);
	TestCheck(Mult(3, 3),   3 * 3);

	TestCheck(Mult(-1, 1), -1 * 1);
	TestCheck(Mult(1, -1),  1 * -1);
	TestCheck(Mult(-1, -1),-1 * -1);
	TestCheck(Mult(1, 1),   1 * 1);
}

void TestDiv() {
	//TestCheck(Div(0, 0),	0 / 0); Does not support errors for /0

	TestCheck(Div(0, 1),	0 / 1);
	TestCheck(Div(0, -1),	0 / -1);
	//TestCheck(Div(1, 0),	1 / 0);
	//TestCheck(Div(-1, 0),-1 / 0);
	
	TestCheck(Div(1, 5),	1 / 5);
	TestCheck(Div(1, -5),	1 / -5);
	TestCheck(Div(-1, 5),  -1 / 5);
	TestCheck(Div(-1, -5), -1 / -5);
	
	TestCheck(Div(2, 4),	2 / 4);
	TestCheck(Div(4, 2),	4 / 2);
	
	TestCheck(Div(-5, -3), -5 / -3);	
	TestCheck(Div(-3, -5), -3 / -5);
	
	TestCheck(Div(-9, 4),  -9 / 4);
	TestCheck(Div(4, -9),	4 / -9);
	TestCheck(Div(-4, 9),  -4 / 9);
	TestCheck(Div(9, -4),	9 / -4);
	
	TestCheck(Div(-3, 3), -3 / 3);
	TestCheck(Div(3, -3),  3 / -3);
	TestCheck(Div(-3, -3),-3 / -3);
	TestCheck(Div(3, 3),   3 / 3);

	TestCheck(Div(-1, 1), -1 / 1);
	TestCheck(Div(1, -1),  1 / -1);
	TestCheck(Div(-1, -1),-1 / -1);
	TestCheck(Div(1, 1),   1 / 1);
}

void TestMod() {
	//TestCheck(Mod(0, 0),   0 % 0);

	TestCheck(Mod(0, 1),   0 % 1);
	TestCheck(Mod(0, -1),  0 % -1);
	//TestCheck(Mod(1, 0),   1 % 0);
	//TestCheck(Mod(-1, 0), -1 % 0);
	
	TestCheck(Mod(1, 5),   1 % 5);
	TestCheck(Mod(1, -5),  1 % -5);
	TestCheck(Mod(-1, 5), -1 % 5);
	TestCheck(Mod(-1, -5),-1 % -5);
	
	TestCheck(Mod(2, 4),   2 % 4);
	TestCheck(Mod(4, 2),   4 % 2);
	
	TestCheck(Mod(-5, -3),-5 % -3);	
	TestCheck(Mod(-3, -5),-3 % -5);
	
	TestCheck(Mod(-9, 4), -9 % 4);
	TestCheck(Mod(4, -9),  4 % -9);
	TestCheck(Mod(-4, 9), -4 % 9);
	TestCheck(Mod(9, -4),  9 % -4);
	
	TestCheck(Mod(-3, 3), -3 % 3);
	TestCheck(Mod(3, -3),  3 % -3);
	TestCheck(Mod(-3, -3),-3 % -3);
	TestCheck(Mod(3, 3),   3 % 3);

	TestCheck(Mod(-1, 1), -1 % 1);
	TestCheck(Mod(1, -1),  1 % -1);
	TestCheck(Mod(-1, -1),-1 % -1);
	TestCheck(Mod(1, 1),   1 % 1);
}

void TestEq() {
	TestCheck(Eq(0, 0),	  0 == 0);

	TestCheck(Eq(0, 1),	  0 == 1);
	TestCheck(Eq(0, -1),  0 == -1);
	TestCheck(Eq(1, 0),	  1 == 0);
	TestCheck(Eq(-1, 0), -1 == 0);
	
	TestCheck(Eq(1, 5),	  1 == 5);
	TestCheck(Eq(1, -5),  1 == -5);
	TestCheck(Eq(-1, 5), -1 == 5);
	TestCheck(Eq(-1, -5),-1 == -5);
	
	TestCheck(Eq(2, 4),	  2 == 4);
	TestCheck(Eq(4, 2),	  4 == 2);
	
	TestCheck(Eq(-5, -3),-5 == -3);	
	TestCheck(Eq(-3, -5),-3 == -5);
	
	TestCheck(Eq(-9, 4), -9 == 4);
	TestCheck(Eq(4, -9),  4 == -9);
	TestCheck(Eq(-4, 9), -4 == 9);
	TestCheck(Eq(9, -4),  9 == -4);
	
	TestCheck(Eq(-3, 3), -3 == 3);
	TestCheck(Eq(3, -3),  3 == -3);
	TestCheck(Eq(-3, -3),-3 == -3);
	TestCheck(Eq(3, 3),   3 == 3);

	TestCheck(Eq(-1, 1), -1 == 1);
	TestCheck(Eq(1, -1),  1 == -1);
	TestCheck(Eq(-1, -1),-1 == -1);
	TestCheck(Eq(1, 1),   1 == 1);
}

void TestGt() {
	TestCheck(Gt(0, 0),	  0 > 0);

	TestCheck(Gt(0, 1),	  0 > 1);
	TestCheck(Gt(0, -1),  0 > -1);
	TestCheck(Gt(1, 0),	  1 > 0);
	TestCheck(Gt(-1, 0), -1 > 0);
	
	TestCheck(Gt(1, 5),	  1 > 5);
	TestCheck(Gt(1, -5),  1 > -5);
	TestCheck(Gt(-1, 5), -1 > 5);
	TestCheck(Gt(-1, -5),-1 > -5);
	
	TestCheck(Gt(2, 4),	  2 > 4);
	TestCheck(Gt(4, 2),	  4 > 2);
	
	TestCheck(Gt(-5, -3),-5 > -3);	
	TestCheck(Gt(-3, -5),-3 > -5);
	
	TestCheck(Gt(-9, 4), -9 > 4);
	TestCheck(Gt(4, -9),  4 > -9);
	TestCheck(Gt(-4, 9), -4 > 9);
	TestCheck(Gt(9, -4),  9 > -4);
	
	TestCheck(Gt(-3, 3), -3 > 3);
	TestCheck(Gt(3, -3),  3 > -3);
	TestCheck(Gt(-3, -3),-3 > -3);
	TestCheck(Gt(3, 3),   3 > 3);

	TestCheck(Gt(-1, 1), -1 > 1);
	TestCheck(Gt(1, -1),  1 > -1);
	TestCheck(Gt(-1, -1),-1 > -1);
	TestCheck(Gt(1, 1),   1 > 1);
}

void TestLt() {
	TestCheck(Lt(0, 0),	  0 < 0);

	TestCheck(Lt(0, 1),	  0 < 1);
	TestCheck(Lt(0, -1),  0 < -1);
	TestCheck(Lt(1, 0),	  1 < 0);
	TestCheck(Lt(-1, 0), -1 < 0);
	
	TestCheck(Lt(1, 5),	  1 < 5);
	TestCheck(Lt(1, -5),  1 < -5);
	TestCheck(Lt(-1, 5), -1 < 5);
	TestCheck(Lt(-1, -5),-1 < -5);
	
	TestCheck(Lt(2, 4),	  2 < 4);
	TestCheck(Lt(4, 2),	  4 < 2);
	
	TestCheck(Lt(-5, -3),-5 < -3);	
	TestCheck(Lt(-3, -5),-3 < -5);
	
	TestCheck(Lt(-9, 4), -9 < 4);
	TestCheck(Lt(4, -9),  4 < -9);
	TestCheck(Lt(-4, 9), -4 < 9);
	TestCheck(Lt(9, -4),  9 < -4);
	
	TestCheck(Lt(-3, 3), -3 < 3);
	TestCheck(Lt(3, -3),  3 < -3);
	TestCheck(Lt(-3, -3),-3 < -3);
	TestCheck(Lt(3, 3),   3 < 3);

	TestCheck(Lt(-1, 1), -1 < 1);
	TestCheck(Lt(1, -1),  1 < -1);
	TestCheck(Lt(-1, -1),-1 < -1);
	TestCheck(Lt(1, 1),   1 < 1);
}

void TestAll() {
	TestMult();
	TestDiv();
	TestMod();
	TestEq();
	TestGt();
	TestLt();
}