#pragma once

// Defines constants/functions equivalent to those in the esolang starting environment
#define one 1
#define zero (1 + (-1))
#define t 1
#define f 0

int Not(int x);
int Eq(int x, int y);
int Gt(int x, int y);
int Lt(int x, int y);

int Mult(int x, int y);
int Div(int x, int y);
int Mod(int x, int y);