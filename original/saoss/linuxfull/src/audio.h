/*
 * $Id: audio.h,v 1.2 2003/04/24 08:37:57 tgo Exp $
 *
 *
 * $Log: audio.h,v $
 * Revision 1.2  2003/04/24 08:37:57  tgo
 * no message
 *
 * Revision 1.1  2003/04/20 08:49:13  tgo
 * Uusiks vähän rakennetta =)
 *
 *
 * $Date: 2003/04/24 08:37:57 $
 *
 */

#ifndef __AUDIO_H_INCLUDED__
#define __AUDIO_H_INCLUDED__


#define AUDIO_FMOD
//#define AUDIO_NONE




#ifdef AUDIO_FMOD
#include "fmod.h"
#endif


class Sound;

typedef void(* STOPCALLBACK)(Sound *sound);

#define SOUNDTYPE_AUTODETECT 0
#define SOUNDTYPE_MODULE 1
#define SOUNDTYPE_STREAM 2
#define SOUNDTYPE_SAMPLE 3

class Sound{
private:
#ifdef AUDIO_FMOD
	int type;
	FMUSIC_MODULE *module;
	FSOUND_STREAM *stream;
	FSOUND_SAMPLE *sample;
	int channel;
#endif
	bool loops;
	bool finished;
	bool running;
	float volume;
	STOPCALLBACK stopcallback;
	bool isFinished(void);

public:
	Sound(Sound *source);
	Sound(char *filename);
	Sound(char *filename, int type);
	Sound(char *filename, bool loops);
	Sound(char *filename, int type, bool loops);
	void load(char *filename, int type, bool loops);
	void play();
	void stop();
	void setStopCallback(STOPCALLBACK callback);
	void setVolume(float volume);
	
	//Do not use methods below
	void setFinished(void);
	void update(void);
};

void initAudio(void);
void uninitAudio(void);
void updateAudio(void);
extern char working;

#endif
