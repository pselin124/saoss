#ifdef WIN32
#include <io.h>
#else
#include <glob.h>
#endif

#include "main.h"

#include "glapi.h"
#include "scene.h"
#include "Sets.h"
#include "net.h"
#include "LippuStart.h"


unsigned long calculatedframes=0,drawedframes=0;

int players;
int ai;
int screenwidth=1024;
int screenheight=768;
int screenbpp;
int maksimipadit, maksimiepadit;
int sirpaleita=1500;
int alkusirpaleita=120;
double fullcredits;
Pati *asteroidi;
Sound *backgroundsong;
Sets *sets;
Pati *pikkumies;
//GLfloat v0[3], v1[3],v2[3], v3[3], v4[3], v5[3], 6[3], v7[3];
Player *player;
Alus *alus;
Sirpale *sirpale;
Star *star;
bool lctrl=false,rctrl=false;
double camposx, camposy, camposz;
bool entterpressed, escpressed,spacepressed;
bool pistepeli, ekapisteframe;
bool musiikkipaalla,efektitpaalla;
//Mix_Music *music1, *music2, *music3, *music4, *music5;
char **names;
//Mix_Music **musics;
/*#ifdef WIN32
SMPEG **mp3s;
#endif*/
int count ;
#ifdef WIN32
HDC		hDC=NULL;							// Private GDI Device Context
HGLRC		hRC=NULL;							// Permanent Rendering Context
HWND		hWnd=NULL;							// Holds Our Window Handle
HINSTANCE	hInstance;							// Holds The Instance Of The Application
#else
#define TRUE true;
#define FALSE false;
#endif

bool	keys[256];								// Array Used For The Keyboard Routine
bool	active=TRUE;								// Window Active Flag
bool	fullscreen=TRUE;							// Fullscreen Flag

GLfloat	xrot;									// X Rotation ( NEW )
GLfloat	yrot;									// Y Rotation ( NEW )
GLfloat	zrot;									// Z Rotation ( NEW )

GLuint	texture[45];
GLuint	*jalkitexture;
int nykymusa;
int ekakerta;

float calculatefps;
int currenttime;
int framecounter,oldframecounter;
int framesdrawn;
int skipframes;
int starttime;
bool alotettu=false;

int debugcounter;
SDL_Rect **modes;


void startMusic(void){
//	else 
	ekakerta=0;
	double temp=random(1000)/1000.0f*(double)count;
	while((int)temp+1==nykymusa){
		temp=random(1000)/1000.0f*(double)count;
	}
	//SMPEG_play( mp3s[(int)temp]);
//	Mix_PlayMusic(musics[(int)temp], 0);

//	Mix_HookMusicFinished(musicDone);
}

void musicDone(){
//	if(ekakerta==0) 
//	Mix_HaltMusic();
//	startMusic();
}

void musicloppu(){
//	Mix_HaltMusic();
}

/*AUX_RGBImageRec *LoadBMP(char *Filename){
	return auxDIBImageLoad(Filename);				// Load The Bitmap And Return A Pointer

}
*/


void test(Sound *sound){
	if(alotettu){
		int gah=random(count);
		if(count>0 && musiikkipaalla){
			backgroundsong=new Sound(names[gah]);
			backgroundsong->play();
			backgroundsong->setStopCallback(test);
			backgroundsong->setVolume(sets->musicvolume/100.0f);
		}
	}else{
			backgroundsong=new Sound("musics\\StartMusic\\start.mp3");
			backgroundsong->play();
			backgroundsong->setStopCallback(test);
			backgroundsong->setVolume(sets->musicvolume/100.0f);

	}
}


void exitProgram(int code){
	printf("frames drawn: %i\n",framesdrawn);
	//uninitAudio();
	SDLNet_Quit();
	SDL_Quit();
	printf("frames drawn: %i, calculated frames:%i\n",drawedframes,calculatedframes);
	printf("time: %i secs\n",(currenttime)/1000);
	printf("drawn fps: %f\n", (float)drawedframes/(float)calculatedframes*(float)calculatefps);
	printf("calculated fps: %i\n", (int) calculatefps);
	if(results>0) printf("received %i bytes gamedata in %i packets %i bytes/packet\n"
		,results,packets,results/packets);
	exit(code);
}

void handleKeyup(SDL_keysym *keysym){
	switch( keysym->sym ) {
	case SDLK_UP:
		if(player!=NULL){
			player[0].pylos=false;
		}
		break;
	case SDLK_DOWN:
		if(player!=NULL){
			player[0].palas=false;
		}
		break;
	case SDLK_LEFT:
		if(player!=NULL){
			player[0].pleft=false;
		}
		break;
	case SDLK_RIGHT:
		if(player!=NULL){
			player[0].pright=false;
		}
		break;
	case SDLK_RSHIFT:
		if(player!=NULL){
			player[0].pfire=false;
		}
		break;
	case SDLK_RCTRL:
		if(player!=NULL){
			player[0].pease=false;
		}
		rctrl=false;
		break;

	case SDLK_w:
		if(players>1){
			player[1].pylos=false;
		}
		break;
	case SDLK_s:
		if(players>1){
			player[1].palas=false;
		}
		break;
	case SDLK_a:
		if(players>1){
			player[1].pleft=false;
		}
		break;
	case SDLK_d:
		if(players>1){
			player[1].pright=false;
		}
		break;
	case SDLK_LSHIFT:
		if(player!=NULL){
			player[1].pfire=false;
		}
		break;
	case SDLK_LCTRL:
		if(player!=NULL){
			player[1].pease=false;
		}
		lctrl=false;
		break;

	case SDLK_i:
		if(players>2){
			player[2].pylos=false;
		}
		break;
	case SDLK_k:
		if(players>2){
			player[2].palas=false;
		}
		break;
	case SDLK_j:
		if(players>2){
			player[2].pleft=false;
		}
		break;
	case SDLK_l:
		if(players>2){
			player[2].pright=false;
		}
		break;
	case SDLK_b:
		if(players>2){
			player[2].pfire=false;
		}
		break;
	case SDLK_n:
		if(players>2){
			player[2].pease=false;
		}
		break;


	case SDLK_KP8:
		if(players>3){
			player[3].pylos=false;
		}
		break;
	case SDLK_KP5:
		if(players>3){
			player[3].palas=false;
		}
		break;
	case SDLK_KP4:
		if(players>3){
			player[3].pleft=false;
		}
		break;
	case SDLK_KP6:
		if(players>3){
			player[3].pright=false;
		}
		break;
	case SDLK_KP0:
		if(players>3){
			player[3].pfire=false;
		}
		break;
	case SDLK_KP_PERIOD:
		if(players>3){
			player[3].pease=false;
		}
		break;

	default:
		break;
	}
}

unsigned char luettunappi,modifier;
SDLKey sdlnappi;

void handleKeydown(SDL_keysym *keysym){
	if(luetaan>0 && keysym->sym!=SDLK_RCTRL && keysym->sym!=SDLK_LCTRL
		 && keysym->sym!=SDLK_RSHIFT && keysym->sym!=SDLK_LSHIFT){
		if(keysym->sym==SDLK_RETURN){
			entterpressed=true;
		}else if(keysym->sym==SDLK_ESCAPE){
			escpressed=true;
		}else{
			luettunappi = keysym->unicode & 0x7F;
			luetaan=2;
		}
		modifier=keysym->mod;
		sdlnappi=keysym->sym;
		printf("k=%i\n",keysym->sym);
	}
	else switch( keysym->sym ) {
	case SDLK_ESCAPE:
		escpressed=true;
//		exitProgram(0);
		break;
	case SDLK_SPACE:
		spacepressed=true;
		break;
	case SDLK_RETURN:
		entterpressed=true;
		break;
	case SDLK_UP:
		if(player!=NULL){
			player[0].pylos=true;
		}
		break;
	case SDLK_DOWN:
		if(player!=NULL){
			player[0].palas=true;
		}
		break;
	case SDLK_LEFT:
		if(player!=NULL){
			player[0].pleft=true;
		}
		break;
	case SDLK_RIGHT:
		if(player!=NULL){
			player[0].pright=true;
		}
		break;
	case SDLK_RSHIFT:
		if(player!=NULL){
			player[0].pfire=true;
		}
		break;
	case SDLK_RCTRL:
		if(player!=NULL){
			player[0].pease=true;
		}
		rctrl=true;
		break;


	case SDLK_w:
		if(players>1){
			player[1].pylos=true;
		}
		break;
	case SDLK_s:
		if(players>1){
			player[1].palas=true;
		}
		break;
	case SDLK_a:
		if(players>1){
			player[1].pleft=true;
		}
		break;
	case SDLK_d:
		if(players>1){
			player[1].pright=true;
		}
		break;
	case SDLK_LSHIFT:
		if(players>1){
			player[1].pfire=true;
		}
		break;
	case SDLK_LCTRL:
		if(players>1){
			player[1].pease=true;
		}
		lctrl=true;
		break;
	
	case SDLK_i:
		if(players>2){
			player[2].pylos=true;
		}
		break;
	case SDLK_k:
		if(players>2){
			player[2].palas=true;
		}
		break;
	case SDLK_j:
		if(players>2){
			player[2].pleft=true;
		}
		break;
	case SDLK_l:
		if(players>2){
			player[2].pright=true;
		}
		break;
	case SDLK_b:
		if(players>2){
			player[2].pfire=true;
		}
		break;
	case SDLK_n:
		if(players>2){
			player[2].pease=true;
		}
		break;

	case SDLK_KP8:
		if(players>3){
			player[3].pylos=true;
		}
		break;
	case SDLK_KP5:
		if(players>3){
			player[3].palas=true;
		}
		break;
	case SDLK_KP4:
		if(players>3){
			player[3].pleft=true;
		}
		break;
	case SDLK_KP6:
		if(players>3){
			player[3].pright=true;
		}
		break;
	case SDLK_KP0:
		if(players>3){
			player[3].pfire=true;
		}
		break;
	case SDLK_KP_PERIOD:
		if(players>3){
			player[3].pease=true;
		}
		break;
	default:
		break;
	}
}

void processEvents(void){
	SDL_Event event;
	while (SDL_PollEvent(&event)){
		switch (event.type){
		case SDL_KEYUP:
			handleKeyup(&event.key.keysym);
			break;
		case SDL_KEYDOWN:
			handleKeydown(&event.key.keysym);
			break;
		case SDL_VIDEORESIZE:
			screenwidth=event.resize.w;
			screenheight=event.resize.h;
			ReSizeGLScene(screenwidth,screenheight);
			break;
		case SDL_QUIT:
			exitProgram(0);
			break;
		}
	}
}

int modenumber;


int main(int argc, char *argv[]){
	maksimipadit=10000;
	maksimiepadit=10000;
	sets=new Sets;
	int gah=sets->loadFile("sets.txt");
//	int jaa;
/*	for(jaa=0;jaa<8;jaa++){
		if(sets->ihminen[jaa]==true) printf("joo ");
		else printf("ei ");
	}*/
	screenwidth=sets->width;
	screenheight=sets->height;
	ai=sets->ai;
	fullcredits=sets->credits;
	entterpressed=false;
	spacepressed=false;
	escpressed=false;
	ekakerta=1;
	if(sets->musics>0) musiikkipaalla=true;
	else musiikkipaalla=false;
	if(sets->efects>0) efektitpaalla=true;
	else efektitpaalla=false;
	if(musiikkipaalla || efektitpaalla) working=true;
	players=sets->playermaara;
	player=new Player[MAXNETPLAYERS];
	printf("musics:%i, effects:%i", musiikkipaalla, efektitpaalla);

	int audio_rate = 44100;
	Uint16 audio_format = AUDIO_S16; 
	int audio_channels = 2;
	int audio_buffers = 2048;

	SDL_Init(SDL_INIT_VIDEO);

		if(SDLNet_Init()==-1) {
			printf("SDLNet_Init: %s\n", SDLNet_GetError());
			exit(2);
		}
	const SDL_VideoInfo *info=SDL_GetVideoInfo();
	if (!info){
		printf("Could not get video info with SDL: %s.\n",SDL_GetError());
		exitProgram(2);
	}



modes=SDL_ListModes(NULL, SDL_FULLSCREEN|SDL_HWSURFACE);
modenumber=0;
if(modes == (SDL_Rect **)0){
  printf("No full-screen modes available!\n");
}

if(modes == (SDL_Rect **)-1){
  printf("All resolutions available.\n");
}
else{
  printf("Available Modes\n");
	int ji;
  for(ji=0;modes[ji];++ji){
    printf("  %d x %d\n", modes[ji]->w, modes[ji]->h);
		if(modes[ji]->w <= screenwidth && modenumber==0){
			modenumber=ji;
		}
	}	
}


	screenbpp=info->vfmt->BitsPerPixel;
	int videomem=info->video_mem;
	printf("Video memory available %i kb\n",videomem);
	SDL_GL_SetAttribute(SDL_GL_RED_SIZE,5);
	SDL_GL_SetAttribute(SDL_GL_GREEN_SIZE,5);
	SDL_GL_SetAttribute(SDL_GL_BLUE_SIZE,5);
	SDL_GL_SetAttribute(SDL_GL_DEPTH_SIZE,16);
	SDL_GL_SetAttribute(SDL_GL_DOUBLEBUFFER,1);


	atexit(SDL_Quit);

	SDL_WM_SetCaption("Saoss",NULL);
	SDL_WM_SetIcon(SDL_LoadBMP("icon.bmp"), NULL);

	//printf("Trying to set %ix%i resolution\n",screenwidth, screenheight);
	if(sets->fullscreen>0){
		if (!SDL_SetVideoMode(screenwidth,screenheight,screenbpp,SDL_RESIZABLE | SDL_OPENGL | SDL_FULLSCREEN)){
			fprintf(stderr,"Couldn't set %i*%i*%i opengl video mode: %s\n",screenwidth,screenheight,screenbpp,SDL_GetError());
			if(!SDL_SetVideoMode(640,480,screenbpp,SDL_RESIZABLE | SDL_OPENGL)){
				exitProgram(-1);
			}else{
				sets->width=640;
				sets->height=480;
				sets->fullscreen=0;
				screenwidth=640;
				screenheight=480;
			}
		}
		SDL_ShowCursor(SDL_DISABLE);
	}else if (!SDL_SetVideoMode(screenwidth,screenheight,screenbpp,SDL_RESIZABLE | SDL_OPENGL)){// | SDL_FULLSCREEN)){
		fprintf(stderr,"Couldn't set %i*%i*%i opengl video mode: %s\n",screenwidth,screenheight,screenbpp,SDL_GetError());
		exitProgram(-1);
//		SDL_ShowCursor(SDL_DISABLE);
		SDL_ShowCursor(SDL_ENABLE);
	}

	SDL_Surface *surface=SDL_GetVideoSurface();
	screenwidth=surface->w;
	screenheight=surface->h;
	printf("Initialized in %ix%i resolution\n",screenwidth, screenheight);

       


	struct timeb timebuf;
	ftime(&timebuf);
	srand((unsigned)timebuf.millitm);
	starttime=SDL_GetTicks();
	oldframecounter=0;
	framesdrawn=0;
	calculatefps=sets->fps;
	alus=new Alus[aluksia];
	int i;



	sirpale=new Sirpale[sirpaleita];
	star=new Star[stars];
	memset(star,0,sizeof(Star)*stars);
	memset(sirpale,0,sizeof(Sirpale)*sirpaleita);
	
	
#ifdef WIN32

	//etsi *.mp3
	struct _finddata_t fileinfo;
	long handle = _findfirst("musics\\*.mp3", &fileinfo);
	int rc = handle;
	count = 0;
	while (rc != -1){
		if ((fileinfo.attrib&32) != 0) count++;
		rc = _findnext(handle, &fileinfo);
	}
	_findclose(handle);

	//etsi *.xm
	handle = _findfirst("musics\\*.xm", &fileinfo);
	rc = handle;
	while (rc != -1){
		if ((fileinfo.attrib&32) != 0) count++;
		rc = _findnext(handle, &fileinfo);
	}
	_findclose(handle);

	
	//etsi *.s3m
	handle = _findfirst("musics\\*.s3m", &fileinfo);
	rc = handle;
	while (rc != -1){
		if ((fileinfo.attrib&32) != 0) count++;
		rc = _findnext(handle, &fileinfo);
	}
	_findclose(handle);

	//etsi *.mod
	handle = _findfirst("musics\\*.mod", &fileinfo);
	rc = handle;
	while (rc != -1){
		if ((fileinfo.attrib&32) != 0) count++;
		rc = _findnext(handle, &fileinfo);
	}
	_findclose(handle);


	//etsi *.it
	handle = _findfirst("musics\\*.it", &fileinfo);
	rc = handle;
	while (rc != -1){
		if ((fileinfo.attrib&32) != 0) count++;
		rc = _findnext(handle, &fileinfo);
	}
	_findclose(handle);

	names = new char*[count];

	printf("Loading musics...\n");
	//nimet ylös *.mp3
	handle = _findfirst("musics\\*.mp3", &fileinfo);
	count = 0;
	rc = handle;
	while (rc != -1){
	//	if ((fileinfo.attrib&32) != 0){
			char *nimi = new char[strlen("musics\\") + strlen(fileinfo.name) + 1];
			strcpy(nimi, "musics\\");
			strcat(nimi, fileinfo.name);
			printf(" %s\r\n",nimi);
			names[count]=nimi;
			count++;
	//	}
		rc = _findnext(handle, &fileinfo);
	}

	//nimet ylös *.xm
	handle = _findfirst("musics\\*.xm", &fileinfo);
	rc = handle;
	while (rc != -1){
	//	if ((fileinfo.attrib&32) != 0){
			char *nimi = new char[strlen("musics\\") + strlen(fileinfo.name) + 1];
			strcpy(nimi, "musics\\");
			strcat(nimi, fileinfo.name);
			printf(" %s\r\n",nimi);
			names[count]=nimi;
			count++;
	//	}
		rc = _findnext(handle, &fileinfo);
	}

	//nimet ylös *.s3m
	handle = _findfirst("musics\\*.s3m", &fileinfo);
	rc = handle;
	while (rc != -1){
	//	if ((fileinfo.attrib&32) != 0){
			char *nimi = new char[strlen("musics\\") + strlen(fileinfo.name) + 1];
			strcpy(nimi, "musics\\");
			strcat(nimi, fileinfo.name);
			printf(" %s\r\n",nimi);
			names[count]=nimi;
			count++;
	//	}
		rc = _findnext(handle, &fileinfo);
	}
	//nimet ylös *.mod
	handle = _findfirst("musics\\*.mod", &fileinfo);
	rc = handle;
	while (rc != -1){
//		if ((fileinfo.attrib&32) != 0){
			char *nimi = new char[strlen("musics\\") + strlen(fileinfo.name) + 1];
			strcpy(nimi, "musics\\");
			strcat(nimi, fileinfo.name);
			printf(" %s\r\n",nimi);
			names[count]=nimi;
			count++;
//		}
		rc = _findnext(handle, &fileinfo);
	}

	//nimet ylös *.it
	handle = _findfirst("musics\\*.it", &fileinfo);
	rc = handle;
	while (rc != -1){
	//	if ((fileinfo.attrib&32) != 0){
			char *nimi = new char[strlen("musics\\") + strlen(fileinfo.name) + 1];
			strcpy(nimi, "musics\\");
			strcat(nimi, fileinfo.name);
			printf(" %s\r\n",nimi);
			names[count]=nimi;
			count++;
//		}
		rc = _findnext(handle, &fileinfo);
	}

#else // UNIX tms.

#define NOT_ENOUGH_MEMORY(a) { \
	fprintf(stderr, "Ei muistia tai jotain muuta hassua tuli (%d)? :(\n", a); \
	exit(255); }
	
	glob_t globt;
	globt.gl_offs = 0;

	int ret = glob("musics/*.mp3", GLOB_DOOFFS, 0, &globt);
	ret |= glob("musics/*.s3m", GLOB_DOOFFS | GLOB_APPEND, 0, &globt);
	ret |= glob("musics/*.xm",  GLOB_DOOFFS | GLOB_APPEND, 0, &globt);
	ret |= glob("musics/*.mod", GLOB_DOOFFS | GLOB_APPEND, 0, &globt);
	ret |= glob("musics/*.it",  GLOB_DOOFFS | GLOB_APPEND, 0, &globt);
	if (ret!=0 && ret!=GLOB_NOMATCH) NOT_ENOUGH_MEMORY(ret);

	for (count=0; globt.gl_pathv[count]; count++);
	names = new char *[count];

	for (i=0; i<count; i++)
	{
		char *name = (char *) strdup(globt.gl_pathv[i]);
		if (!name) NOT_ENOUGH_MEMORY(0);
		names[i] = name;
		printf(" %s\r\n",name);
	}
	
	globfree(&globt);

#endif
	
	initAudio();

	alota();
	LoadGLFont();
	alotettu=false;
	if(musiikkipaalla){
#ifdef WIN32
		backgroundsong=new Sound("musics\\StartMusic\\start.mp3");
#else
		backgroundsong=new Sound("musics/StartMusic/start.mp3");
#endif
		backgroundsong->setVolume(sets->musicvolume/100.0f);
 		backgroundsong->setStopCallback(test);
		backgroundsong->play();
		backgroundsong->stop();
		backgroundsong->play();
	}
	AlotaLippu();

	if(musiikkipaalla){
		backgroundsong->stop();
	}
	alotettu=true;
//Pelin opengl-asetukset
	setupOpengl(screenwidth,screenheight);
	framecounter=0;


	camposx=0; camposy=0; camposz=20;

	
	nykymusa=0;
	if (nykymusa==0) nykymusa++;
	//backgroundsong = new Sound(names[0]);
	bool eikiva=true;
	if(musiikkipaalla){
		printf("Starting musics, Song:");
		int gahb=random(count);
		if(count>0){
			printf("%s",names[gahb]);
			backgroundsong = new Sound(names[gahb]);
			//backgroundsong->play();
 			backgroundsong->setStopCallback(test);
			backgroundsong->setVolume(sets->musicvolume);
			eikiva=false;

//			printf(".");
		}
		printf("\n");
	}

	if(!eikiva){
		//backgroundsong->stop();
		//backgroundsong->play();
	}

/*	printf("%s",names[0]);
	printf("\r\n");
	printf("%s",names[1]);
	printf("\r\n");*/

	int j;
	for(i=0;i<MAXNETPLAYERS;i++){
		double temp3=random(1000);
		player[i].asetaAlus(alus[1]);
		player[i].uudestaan(alus[(int)(temp3/1000.0f*(double)aluksia)]);
		sprintf(netnick[i],"player %i",i+1);
		for(j=0;j<MAXNETPLAYERS;j++){
			sprintf(tcpnick[i][j],"player %i",j+1);
		}
	}

	entterpressed=false;
	escpressed=false;
	pistepeli=false;

	/* Mainloop */
	mainmenu();

	/* Normal exit */
	printf("frames drawn: %i\n",framesdrawn);
	//uninitAudio();
	SDLNet_Quit();
	SDL_Quit();
	printf("frames drawn: %i, calculated frames:%i\n",drawedframes,calculatedframes);
	printf("time: %i secs\n",(currenttime)/1000);
	printf("drawn fps: %f\n", (float)drawedframes/(float)calculatedframes*(float)calculatefps);
	printf("calculated fps: %i\n", (int) calculatefps);
	if(results>0) printf("received %i bytes gamedata in %i packets %i bytes/packet\n"
		,results,packets,results/packets);

	return 0;
}

char netnick[MAXNETPLAYERS][11];
char tcpnick[MAXNETPLAYERS][MAXNETPLAYERS][11];
