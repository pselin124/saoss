#ifndef _alus_h_included
#define	_alus_h_included

#include <stdlib.h>
#include <stdio.h>

#include <string.h>
#include <stdlib.h>
#include <math.h>
#include <sys/types.h>
#include <time.h>
#include <math.h>

#ifdef WIN32
#include <windows.h>
#endif

#include <SDL.h>
#include <SDL_opengl.h>
//#include <SDL_mixer.h>
//#include "h/main.h"
//#include <GL/glaux.h>
#include "audio.h"

//typedef bool boolean;

#ifndef WIN32
	#define itoa(a, b, c) snprintf(b, c, "%d", a)
#endif


class Alus{
public:
	Alus();
	~Alus();
	void piirra(float x, float y, float z, float kulma, float asekulma, int numero);
	double patienergia, epatienergia;
	Sound *ampumisaani;
	Sound *osumisaani;
	Sound *eampumisaani;
	Sound *eosumisaani;
	GLuint texture, patitexture, texture2,epatitexture;
	bool patihajottaa, epatihajottaa;
	bool epatiVoiHajota, patiVoiHajota;
	double nopeus, paino, kaantonopeus,epatisade;
	double ampumisnopeus, patiaika, maxpadit, maxepadit;
	double patinopeus, patisade, edamage,epatipotkasu;
	double sade, patipotkasu,easefuel, easekesto, easenopeus, easevauhti;
	double damage,energia, jalkikoko, fuel, tankkaus, patifuel;
	int aselaji,easelaji;
	double pistemaara;
	int numero;
	char nimi[15];
private:
};
#endif
