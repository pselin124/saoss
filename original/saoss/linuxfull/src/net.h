#ifndef net_h
#define net_h
#define MAXNETPLAYERS 16
#define MAX_MP_CREDITS 9000
#define MAXLEN 64000
#define TIME_OUT_MAARA 40
#define MAXPATIMAARA 30

typedef struct{
	float x,y,energia;
	short aika,numero;
}pikkumiessiirto;

typedef struct{
	short pisteet,numero;
}VictoryPlayer;

typedef struct{
	char gamemode;
	short mpcredits;
	float fps;
}AlkuTiedot;

typedef struct{
	float x,y,kulma,energia,fuel;
	short kuolema, pisteet;
	//bool kaasu, ease, ase, left, right;
	char napit;
}coords;

typedef struct{
	char ylos, alas, left, right, fire, ease;
}napit;

typedef struct{
	float x,y;
}asteroidipox;

typedef struct{
	float x,y;
	short player,kohta;
}limajuttu;

extern int started2;
extern unsigned short netframe,netframe2;
extern pikkumiessiirto siirrot[10];
void hostKasittele(char *msg, int result, int i);
extern bool netGameQuit;
void debugPiirtomenu();
void drawDebugMenu(int dalus);
extern bool debugmode;
extern bool limalaitetaan;
extern limajuttu netlima;
extern int packets,results;
extern char tcpnick[MAXNETPLAYERS][MAXNETPLAYERS][11];
extern char netnick[MAXNETPLAYERS][11];
extern int asteroidipaivitysaika;
extern int paivitettavaasteroidi;
extern asteroidipox prajahdys[MAXNETPLAYERS];
extern bool playerrajahtaa[MAXNETPLAYERS];
extern asteroidipox arajahdys;
extern bool asteroidirajahtaa;
extern int playerslots;
extern int mpcredits;
extern int mptiimi[];
extern int mpstatus[];
extern int connectionstatus[];
extern int soketteja;
extern TCPsocket tcpsoketit[];
extern int kuuntelusoketti;
extern int tila[];
extern TCPsocket joinsoketti;
extern int hosteja;
extern int tila[];
extern int joinaavia;
extern int pelaajiasoketista[];
extern int pelaajasoketista[];
extern int timeout[];
extern int timeoutaika;
extern int timeouttarkistettu;
extern bool gamequited;
extern bool alkujuttusaatu;
extern bool valmiinaalottamaan;
extern bool joinpeli, hostpeli;
extern char playeruudestaan[MAXNETPLAYERS];
extern bool asteroidiuudestaan[asteroideja];

void hostNetKasittele(int i, char *msg, int result);
void startMenu();
void stopMenu();
void paivitaPelaajat();
void kuuntele();
void lahettele();
void hostLahetaPeli();
void joinKuuntelePeli();
void disconnect(int i);
void hostKuuntelePeli();
void lahetaNapitHostille();
void joinPeliNetwork();
void hostPeliNetwork();
void netHostPeli();
void netJoinPeli();
void kuuntelejoin();
void kuuntele(SDLNet_SocketSet set);
void joinwaitKasittele(char *msg, int result);
void hostStart();
void hostQuit();
void joinwaitforstart();
void hostMultiplayer();
void joinMultiplayer();
void hostSettingsMenu();
void kasittelePeliMsg(char *msg, int result);
int compare(char *eka, char *toka, int maara);

#endif
