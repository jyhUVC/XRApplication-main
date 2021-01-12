using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itld_WakeUp_STT
{
    public class ItldVoicetouch
    {
        enum ErrVad
        {
            ITLD_E_SUCCESS = 0,
            ITLD_E_INVALID_ARGUMENT,
            ITLD_E_INSUFFICIENT_MEMORY,
            ITLD_E_INCREASE_MEMORY_ALLOC,
            ITLD_E_FILE_NOT_FOUND,
            ITLD_E_CANNOT_CREATE_FILE,
            ITLD_E_INVALID_FORMAT,
            ITLD_E_INVALID_VALUE,
            ITLD_E_NOT_SUPPORTED,
            ITLD_E_GENERAL,
            ITLD_E_UNKNOWN
        };

        public const int API_ERROR = 0;
        public const int API_NO_KEYWORD_YET = 1;
        public const int API_KEYWORD_DETECTED = 2;
        public const int API_TIMEOUT = 3;


        [StructLayout(LayoutKind.Sequential)]
        public struct itld_vt_api_conf_t
        {
            public int mode;
            public int sensitivity;
            public int top_duration;
            public int timeout_frame_n;
            public int max_insert_pcm_n;
            public int dic_max_word_n;
            public int flag_use_epd;

            public int flag_end_frame_result;
            public int flag_tree_roop;

            public int scorethreshold;
            public int max_active_node_n;
            public int end_margin_frame_n;

            public int eng_check_frame_n;
            public int eng_sort_n;
            public int eng_normal;
            public int eng_noisy;
            public int score_adv_normal;
            public int score_adv_noisy;

            public int epd_thresh_eng_detect;
            public int epd_thresh_eng_speech;
            public int epd_min_frame_n_main_speech;
            public int epd_min_frame_n_sub_speech;
            public int epd_min_frame_n_speech;
            public int epd_end_silence_frame_n;
            public int error_code;
        };
#if UNITY_EDITOR_WIN
        [DllImport("itld_voicetouch_api", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr itld_vt_api_create(ref itld_vt_api_conf_t conf);

        [DllImport("itld_voicetouch_api", CallingConvention = CallingConvention.Cdecl)]
        public static extern int itld_vt_api_reset(IntPtr inst_h);

        [DllImport("itld_voicetouch_api", CallingConvention = CallingConvention.Cdecl)]
        public static extern int itld_vt_api_destroy(ref IntPtr inst_h);

        [DllImport("itld_voicetouch_api", CallingConvention = CallingConvention.Cdecl)]
        public static extern int itld_vt_api_insert_pcm(IntPtr inst_h, byte[] pcm, int sampleCnt);

        [DllImport("itld_voicetouch_api", CallingConvention = CallingConvention.Cdecl)]
        public static extern int itld_vt_api_get_error_code(IntPtr inst_h);

        [DllImport("itld_voicetouch_api", CallingConvention = CallingConvention.Cdecl)]
        public static extern int itld_vt_api_get_detect_duration(IntPtr inst_h);
# else 
        [DllImport("itld_voicetouch_api-jni", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr itld_vt_api_create(ref itld_vt_api_conf_t conf);

        [DllImport("itld_voicetouch_api-jni", CallingConvention = CallingConvention.Cdecl)]
        public static extern int itld_vt_api_reset(IntPtr inst_h);

        [DllImport("itld_voicetouch_api-jni", CallingConvention = CallingConvention.Cdecl)]
        public static extern int itld_vt_api_destroy(ref IntPtr inst_h);

        [DllImport("itld_voicetouch_api-jni", CallingConvention = CallingConvention.Cdecl)]
        public static extern int itld_vt_api_insert_pcm(IntPtr inst_h, byte[] pcm, int sampleCnt);

        [DllImport("itld_voicetouch_api-jni", CallingConvention = CallingConvention.Cdecl)]
        public static extern int itld_vt_api_get_error_code(IntPtr inst_h);

        [DllImport("itld_voicetouch_api-jni", CallingConvention = CallingConvention.Cdecl)]
        public static extern int itld_vt_api_get_detect_duration(IntPtr inst_h);
#endif


        itld_vt_api_conf_t vt_conf;
        // class member, methods
        public ItldVoicetouch(int sens)
        {
            GenerateNewConfig(sens);
        }

        IntPtr vt_h;
        int detectedDuration;

        bool started;
        const int SUCCESS = 1;
        const int FAIL = 0;

        public int create()
        {
            vt_h = itld_vt_api_create(ref vt_conf);
            if (vt_h != IntPtr.Zero)
            {
                started = true;
            }
            else
            {
                started = false;
                return FAIL;
            }
            return SUCCESS;
        }
        public int destroy()
        {
            itld_vt_api_destroy(ref vt_h);
            started = false;
            return SUCCESS;
        }
        public int insertPcm(byte[] pcm)
        {
            int ret;
            if (!started)
            {
                return FAIL;
            }
            ret = itld_vt_api_insert_pcm(vt_h, pcm, pcm.Length>>1);
            return ret;
        }
        public void reset()
        {
            itld_vt_api_reset(vt_h);
        }

        public void reset(int sensitiviy)
        {
            GenerateNewConfig(sensitiviy);
            if(create() == SUCCESS)
                itld_vt_api_reset(vt_h);
        }

        private void GenerateNewConfig(int sensitiviy)
        {
            vt_conf = new itld_vt_api_conf_t();
            vt_conf.mode = 0;
            vt_conf.sensitivity = sensitiviy;
            vt_conf.top_duration = 1;
            vt_conf.timeout_frame_n = 0;
            vt_conf.max_insert_pcm_n = 320;
            vt_conf.dic_max_word_n = 200;
            vt_conf.flag_use_epd = 0;
            vt_conf.flag_end_frame_result = 0;
            vt_conf.flag_tree_roop = 1;
            vt_conf.scorethreshold = 30;
            vt_conf.max_active_node_n = 0;
            vt_conf.end_margin_frame_n = 0;
            vt_conf.eng_check_frame_n = 0;
            vt_conf.eng_sort_n = 30;
            vt_conf.eng_normal = 0;
            vt_conf.eng_noisy = 0;
            vt_conf.score_adv_normal = 10;
            vt_conf.score_adv_noisy = 20;
            vt_conf.epd_thresh_eng_detect = 0;
            vt_conf.epd_thresh_eng_speech = 0;
            vt_conf.epd_min_frame_n_main_speech = 3;
            vt_conf.epd_min_frame_n_sub_speech = 2;
            vt_conf.epd_min_frame_n_speech = 30;
            vt_conf.epd_end_silence_frame_n = 20;
            vt_conf.error_code = 0;
        }

        public int getDetectDuration()
        {
            detectedDuration = itld_vt_api_get_detect_duration(vt_h);
            return detectedDuration;
        }
    }
}
