/*
 * $Id: audio.cpp,v 1.2 2003/04/24 08:37:57 tgo Exp $
 *
 *
 * $Log: audio.cpp,v $
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

#include "main.h"

#include <string.h>
#include <stdio.h>

#include "audio.h"


struct soundlist{
	Sound *sound;
	soundlist *next;
};

soundlist *allsounds = NULL;


Sound::Sound(Sound *source){
	memcpy(this, source, sizeof(Sound));
	soundlist *node = new soundlist;
	node->sound = this;
	node->next = allsounds;
	allsounds = node;
}

Sound::Sound(char *filename){
	load(filename, SOUNDTYPE_AUTODETECT, false);
}

Sound::Sound(char *filename, int type){
	load(filename, type, false);
}

Sound::Sound(char *filename, bool loops){
	load(filename, SOUNDTYPE_AUTODETECT, loops);
}

Sound::Sound(char *filename, int type, bool loops){
	load(filename, type, loops);
}

bool endsWith(char *str1, char *str2){
	char *str3 = str1 + strlen(str1) - strlen(str2);
#ifdef WIN32
	if (stricmp(str3, str2)) return false;
#else
	if (strcasecmp(str3, str2)) return false;
#endif
	else return true;
}

void Sound::load(char *filename, int type, bool loops){
	if (type == SOUNDTYPE_AUTODETECT){
		if (endsWith(filename, "mp3") ||
		    endsWith(filename, "mp2") ||
		    endsWith(filename, "ogg")) type = SOUNDTYPE_STREAM;

		if (endsWith(filename, "wav") ||
		    endsWith(filename, "raw")) type = SOUNDTYPE_SAMPLE;
		
		if (endsWith(filename, "s3m") ||
		    endsWith(filename, "xm") ||
		    endsWith(filename, "it") ||
		    endsWith(filename, "mid") ||
		    endsWith(filename, "rmi") ||
		    endsWith(filename, "sgr") ||
		    endsWith(filename, "mod")) type = SOUNDTYPE_MODULE;
	}
#ifdef AUDIO_FMOD
	if(working){
		this->type = type;
		if (type == SOUNDTYPE_MODULE){
			module = FMUSIC_LoadSong(filename);
			this->loops = false;
		} else if (type == SOUNDTYPE_SAMPLE){
/*			if (loops){
				sample = FSOUND_Sample_Load(FSOUND_FREE, filename, FSOUND_LOOP_NORMAL, 0);
				FSOUND_Sample_SetLoopMode(sample, FSOUND_LOOP_NORMAL);
			} else{
				sample = FSOUND_Sample_Load(FSOUND_FREE, filename, FSOUND_LOOP_OFF, 0);
				FSOUND_Sample_SetLoopMode(sample, FSOUND_LOOP_OFF);
			}*/
			if (loops){
				sample = FSOUND_Sample_Load(FSOUND_FREE, filename, FSOUND_LOOP_NORMAL, 0);
				FSOUND_Sample_SetMode(sample, FSOUND_LOOP_NORMAL);
			} else{
				sample = FSOUND_Sample_Load(FSOUND_FREE, filename, FSOUND_LOOP_OFF, 0);
				FSOUND_Sample_SetMode(sample, FSOUND_LOOP_OFF);
			}
			this->loops = loops;
		} else if (type == SOUNDTYPE_STREAM){
			if (loops){
				stream = FSOUND_Stream_OpenFile(filename, FSOUND_LOOP_NORMAL, 0);
			} else{
				stream = FSOUND_Stream_OpenFile(filename, FSOUND_LOOP_OFF, 0);
			}
			this->loops = loops;
		}
	}
#endif
	stopcallback = NULL;
	soundlist *node = new soundlist;
	node->sound = this;
	node->next = allsounds;
	allsounds = node;
	volume = 1.0;
}


void Sound::play(){
	printf("1\n");
	running = true;
	finished = false;
#ifdef AUDIO_FMOD
	printf("2\n");
	if(working){
	printf("3\n");
		if (type == SOUNDTYPE_MODULE){
	printf("4\n");
			FMUSIC_PlaySong(module);
			FMUSIC_SetMasterVolume(module, volume*256);
		} else if (type == SOUNDTYPE_SAMPLE){
	printf("5\n");
			channel = FSOUND_PlaySound(FSOUND_FREE, sample);
			FSOUND_SetVolume(channel, volume*256);
			if (!loops){
				running = false;
				finished = false;
			}
		} else if (type == SOUNDTYPE_STREAM){
	printf("6\n");
			channel = FSOUND_Stream_Play(FSOUND_FREE, stream);
			FSOUND_SetVolume(channel, volume*256);
		}
	printf("7\n");
	}
#endif
	printf("8\n");
}

void Sound::stop(){
#ifdef AUDIO_FMOD
	if(working){
		if (type == SOUNDTYPE_MODULE){
			FMUSIC_StopSong(module);
		} else if (type == SOUNDTYPE_SAMPLE){
			FSOUND_StopSound(channel);
		} else if (type == SOUNDTYPE_STREAM){
			FSOUND_Stream_Stop(stream);
		}
	}
#endif
}

void Sound::setVolume(float volume){
#ifdef AUDIO_FMOD
	if(working){
		if (type == SOUNDTYPE_MODULE){
			FMUSIC_SetMasterVolume(module, volume*256);
		} else if (type == SOUNDTYPE_SAMPLE){
			FSOUND_SetVolume(channel, volume*256);
		} else if (type == SOUNDTYPE_STREAM){
			FSOUND_SetVolume(channel, volume*256);
		}
	}
#endif
	this->volume = volume;
}

#ifdef AUDIO_FMOD
signed char streamendcallback(FSOUND_STREAM *stream, void *buff, int len, int param){
	Sound *sound = (Sound *)param;
	sound->setFinished();
	return true;
}
#endif

void Sound::setStopCallback(STOPCALLBACK callback){
	stopcallback = callback;
#ifdef AUDIO_FMOD
	if(working){
		if (type == SOUNDTYPE_MODULE){
		} else if (type == SOUNDTYPE_SAMPLE){
			//NOT SUPPORTED
		} else if (type == SOUNDTYPE_STREAM){
			FSOUND_Stream_SetEndCallback(stream, streamendcallback, (int)this);
		}
	}
#endif
}



void Sound::setFinished(void){
	finished = true;
}

bool Sound::isFinished(void){
#ifdef AUDIO_FMOD
	if(working){
		if (type == SOUNDTYPE_MODULE){
			if (FMUSIC_IsFinished(module)) return true;
		} else if (type == SOUNDTYPE_SAMPLE){
			//NOT SUPPORTED
		} else if (type == SOUNDTYPE_STREAM){
			if (finished) return true;
		}
	}
#endif
	return false;
}

void Sound::update(void){
	if (running && isFinished()){
		running = false;
		if (stopcallback != NULL) stopcallback(this);
	}
}


char working=0;
void initAudio(void){
#ifdef AUDIO_FMOD
	working=FSOUND_Init(44100, 64, 0);
	if(!working) printf("Audio system not initialized\r\n");
#endif
}

void uninitAudio(void){
#ifdef AUDIO_FMOD
	if(working) FSOUND_Close();
#endif
}

void updateAudio(void){
	soundlist *node = allsounds;
	while (node != NULL){
		Sound *sound = node->sound;
		sound->update();
		node = node->next;
	}
}
