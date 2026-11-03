#ifndef chat_h
#define chat_h

#define MAXCHATRIVIT 23
#define MAXCHATMERKIT 85
#define MAXCHATKIRJOTUSMERKIT 500

void chatScreen();

void drawChatScreen(int selection);

void alustaChat();

void tyhjennaKirjotusBufferi();

void tyhjennaChatBufferi();

void tyhjennaChatRivi(int rivi);

void lisaaRivi(char *data, int pituus);

void tekstiValmis();

extern int vikakirjain, kirjotettavakirjain, vasenkirjain, ruudunleveys;
extern char chatbuffer[MAXCHATRIVIT][MAXCHATMERKIT];
extern char kirjotusbuffer[MAXCHATKIRJOTUSMERKIT];

#endif