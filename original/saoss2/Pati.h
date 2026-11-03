#ifndef _pati_h
#define _pati_h

typedef struct Pati{
	float x, y, xv, yv,kulma, energia,r,g,b,alku,koko;
	short aika, ampumisviive;
}Pati;

typedef struct{
	float x,y,xv,yv,kulma;
	char pelaaja;
	unsigned short pati;
	float r,g,b;
}netPati;

typedef struct{
	short pati;
	char pelaaja;
}kuolevaPati;

extern char *netpatilahetetaan;
extern char *netepatilahetetaan;

#endif