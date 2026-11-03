#ifndef _MAIN_H_INCLUDED_
#define _MAIN_H_INCLUDED_

#include "Define.h"
#ifdef WIN32
#include <windows.h>
#endif

#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <math.h>
#include <sys/types.h>
#include <time.h>
#include <sys/timeb.h>
#include <SDL.h>
#include <math.h>
#include <GL/gl.h>
#include <GL/glu.h>
//#include <GL/glaux.h>

#ifdef WIN32
#pragma warning(disable:4244) //Disable: conversion from 'double' to 'double', possible loss of data
#pragma warning(disable:4305) //Disable: truncation from 'const double' to 'double'
#endif

void valitseVari(int tiimi);

#ifndef WIN32
#include <SDL_opengl.h>
#endif
//#include <SDL_mixer.h>
#include <SDL_image.h>
#include <SDL_net.h>
//#include <smpeg.h>
#include "fmod.h"
#include "Player.h"
#include "Alus.h"
#include "sirpale.h"
#include "Pati.h"
#include "Kaasujalki.h"
#include "Star.h"
#include "Sets.h"
#include "audio.h"
//#include "ejaija.h"

extern bool dserver;
extern int modenumber;
extern SDL_Rect **modes;
extern float nuolirot;
extern int gamemode;
extern bool lctrl,rctrl;
extern unsigned char luettunappi, modifier;
extern SDLKey sdlnappi;
extern int luetaan;
extern unsigned int list_asteroid;
extern Pati *asteroidi;
extern char **names;
extern Sound *backgroundsong;
extern void test(Sound *sound);
extern Sets *sets;
extern Pati *pikkumies;
extern int screenwidth, screenheight;
extern bool pistepeli,ekapisteframe;
extern void drawVictory(int numero);
extern float gl_pituus(char *vekotin);
extern void gl_println(float x, float y, float z, char *vekotin);
extern void gl_println(float x, float y, float z, char *vekotin, float fonttikoko);
extern void processEvents(void);
extern void exitProgram(int code);
extern float calculatefps;
extern int currenttime;
extern int framecounter,oldframecounter;
extern int count;
extern int framesdrawn;
extern int skipframes;
extern int starttime;
extern unsigned int *list_int;
extern int maksimipadit, maksimiepadit;

extern unsigned long calculatedframes,drawedframes;
extern bool saavalita;
extern void setLists();
extern int ai;
extern double fullcredits;
extern bool entterpressed, escpressed, spacepressed;
extern void drawmenu(int selection);
extern void mainmenu();
extern void asetusmenu();
extern int players;
extern void playerMenu();
extern void onePlayerGameMenu();
extern void drawOnePlayerGameMenu(int selection);
extern void drawPlayerMenu(int selection, int selectionrivi);
extern void drawAsetusmenu(int selection, 
						   int twidht, 
						   int theight, 
						   int tfull, 
						   int tai,
						   int tmus,
						   int tef,
						   int tcr,
						   int tfps,
						   int tmv,
						   int tsv,
						   int tp);
extern Star *star;
extern int ekakerta;
extern void musicDone();
extern int nykymusa;
extern void drawCube();
extern void drawTri();
extern void setSoundVolumes();	
extern bool musiikkipaalla, efektitpaalla;
//extern Mix_Music *music1, *music2;
extern void musicDone();
extern void generateLightFlare(GLuint *texture);
extern void generateTeamJuttu(GLuint *texture);
extern Sirpale *sirpale;
extern double random(double x);
extern void alota();
extern Alus	*alus;
extern Player *player;
#ifdef WIN32
extern HDC		hDC;							// Private GDI Device Context
extern HGLRC		hRC;							// Permanent Rendering Context
extern HWND		hWnd;							// Holds Our Window Handle
extern HINSTANCE	hInstance;							// Holds The Instance Of The Application
#endif
extern double camposx, camposy, camposz;
extern bool	keys[];								// Array Used For The Keyboard Routine
extern bool	active;								// Window Active Flag
extern bool	fullscreen;							// Fullscreen Flag

extern GLfloat	xrot;									// X Rotation ( NEW )
extern GLfloat	yrot;									// Y Rotation ( NEW )
extern GLfloat	zrot;									// Z Rotation ( NEW )

extern GLuint	texture[];								// Storage For One Texture ( NEW )
extern GLuint	tiimitexture;
extern GLuint	*jalkitexture;								// Storage For One Texture ( NEW )

//#ifdef WIN32
//extern LRESULT	CALLBACK WndProc(HWND, UINT, WPARAM, LPARAM);				// Declaration For WndProc
//extern AUX_RGBImageRec *LoadBMP(char *Filename);
//#endif
extern int LoadGLTextures();
extern GLvoid ReSizeGLScene(GLsizei width, GLsizei height);

extern int debugcounter;

#define DEBUG

#ifdef DEBUG
#define DP printf("%s: %i (Debug counter: %i)\n",__FILE__,__LINE__,debugcounter++);
#else
#define DP
#endif

#endif
