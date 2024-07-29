FFmpeg 64-bit static Windows build from www.gyan.dev

Version: 7.0.1-essentials_build-www.gyan.dev

License: GPL v3

Source Code: https://github.com/FFmpeg/FFmpeg/commit/af25a4bfd2

release-essentials build configuration: 

ARCH                      x86 (generic)
big-endian                no
runtime cpu detection     yes
standalone assembly       yes
x86 assembler             nasm
MMX enabled               yes
MMXEXT enabled            yes
3DNow! enabled            yes
3DNow! extended enabled   yes
SSE enabled               yes
SSSE3 enabled             yes
AESNI enabled             yes
AVX enabled               yes
AVX2 enabled              yes
AVX-512 enabled           yes
AVX-512ICL enabled        yes
XOP enabled               yes
FMA3 enabled              yes
FMA4 enabled              yes
i686 features enabled     yes
CMOV is fast              yes
EBX available             yes
EBP available             yes
debug symbols             yes
strip symbols             yes
optimize for size         no
optimizations             yes
static                    yes
shared                    no
postprocessing support    yes
network support           yes
threading support         pthreads
safe bitstream reader     yes
texi2html enabled         no
perl enabled              yes
pod2man enabled           yes
makeinfo enabled          yes
makeinfo supports HTML    yes
xmllint enabled           yes

External libraries:
avisynth                libopencore_amrnb       libvpx
bzlib                   libopencore_amrwb       libwebp
gmp                     libopenjpeg             libx264
gnutls                  libopenmpt              libx265
iconv                   libopus                 libxml2
libaom                  librubberband           libxvid
libass                  libspeex                libzimg
libfontconfig           libsrt                  libzmq
libfreetype             libssh                  lzma
libfribidi              libtheora               mediafoundation
libgme                  libvidstab              sdl2
libgsm                  libvmaf                 zlib
libharfbuzz             libvo_amrwbenc
libmp3lame              libvorbis

External libraries providing hardware acceleration:
amf                     d3d12va                 nvdec
cuda                    dxva2                   nvenc
cuda_llvm               ffnvcodec               vaapi
cuvid                   libmfx
d3d11va                 libvpl

Libraries:
avcodec                 avformat                swresample
avdevice                avutil                  swscale
avfilter                postproc

Programs:
ffmpeg                  ffplay                  ffprobe

Enabled decoders:
aac                     fraps                   pgm
aac_fixed               frwu                    pgmyuv
aac_latm                ftr                     pgssub
aasc                    g2m                     pgx
ac3                     g723_1                  phm
ac3_fixed               g729                    photocd
acelp_kelvin            gdv                     pictor
adpcm_4xm               gem                     pixlet
adpcm_adx               gif                     pjs
adpcm_afc               gremlin_dpcm            png
adpcm_agm               gsm                     ppm
adpcm_aica              gsm_ms                  prores
adpcm_argo              h261                    prosumer
adpcm_ct                h263                    psd
adpcm_dtk               h263i                   ptx
adpcm_ea                h263p                   qcelp
adpcm_ea_maxis_xa       h264                    qdm2
adpcm_ea_r1             h264_cuvid              qdmc
adpcm_ea_r2             h264_qsv                qdraw
adpcm_ea_r3             hap                     qoa
adpcm_ea_xas            hca                     qoi
adpcm_g722              hcom                    qpeg
adpcm_g726              hdr                     qtrle
adpcm_g726le            hevc                    r10k
adpcm_ima_acorn         hevc_cuvid              r210
adpcm_ima_alp           hevc_qsv                ra_144
adpcm_ima_amv           hnm4_video              ra_288
adpcm_ima_apc           hq_hqa                  ralf
adpcm_ima_apm           hqx                     rasc
adpcm_ima_cunning       huffyuv                 rawvideo
adpcm_ima_dat4          hymt                    realtext
adpcm_ima_dk3           iac                     rka
adpcm_ima_dk4           idcin                   rl2
adpcm_ima_ea_eacs       idf                     roq
adpcm_ima_ea_sead       iff_ilbm                roq_dpcm
adpcm_ima_iss           ilbc                    rpza
adpcm_ima_moflex        imc                     rscc
adpcm_ima_mtf           imm4                    rtv1
adpcm_ima_oki           imm5                    rv10
adpcm_ima_qt            indeo2                  rv20
adpcm_ima_rad           indeo3                  rv30
adpcm_ima_smjpeg        indeo4                  rv40
adpcm_ima_ssi           indeo5                  s302m
adpcm_ima_wav           interplay_acm           sami
adpcm_ima_ws            interplay_dpcm          sanm
adpcm_ms                interplay_video         sbc
adpcm_mtaf              ipu                     scpr
adpcm_psx               jacosub                 screenpresso
adpcm_sbpro_2           jpeg2000                sdx2_dpcm
adpcm_sbpro_3           jpegls                  sga
adpcm_sbpro_4           jv                      sgi
adpcm_swf               kgv1                    sgirle
adpcm_thp               kmvc                    sheervideo
adpcm_thp_le            lagarith                shorten
adpcm_vima              lead                    simbiosis_imx
adpcm_xa                libaom_av1              sipr
adpcm_xmd               libgsm                  siren
adpcm_yamaha            libgsm_ms               smackaud
adpcm_zork              libopencore_amrnb       smacker
agm                     libopencore_amrwb       smc
aic                     libopus                 smvjpeg
alac                    libspeex                snow
alias_pix               libvorbis               sol_dpcm
als                     libvpx_vp8              sonic
amrnb                   libvpx_vp9              sp5x
amrwb                   loco                    speedhq
amv                     lscr                    speex
anm                     m101                    srgc
ansi                    mace3                   srt
anull                   mace6                   ssa
apac                    magicyuv                stl
ape                     mdec                    subrip
apng                    media100                subviewer
aptx                    metasound               subviewer1
aptx_hd                 microdvd                sunrast
arbc                    mimic                   svq1
argo                    misc4                   svq3
ass                     mjpeg                   tak
asv1                    mjpeg_cuvid             targa
asv2                    mjpeg_qsv               targa_y216
atrac1                  mjpegb                  tdsc
atrac3                  mlp                     text
atrac3al                mmvideo                 theora
atrac3p                 mobiclip                thp
atrac3pal               motionpixels            tiertexseqvideo
atrac9                  movtext                 tiff
aura                    mp1                     tmv
aura2                   mp1float                truehd
av1                     mp2                     truemotion1
av1_cuvid               mp2float                truemotion2
av1_qsv                 mp3                     truemotion2rt
avrn                    mp3adu                  truespeech
avrp                    mp3adufloat             tscc
avs                     mp3float                tscc2
avui                    mp3on4                  tta
bethsoftvid             mp3on4float             twinvq
bfi                     mpc7                    txd
bink                    mpc8                    ulti
binkaudio_dct           mpeg1_cuvid             utvideo
binkaudio_rdft          mpeg1video              v210
bintext                 mpeg2_cuvid             v210x
bitpacked               mpeg2_qsv               v308
bmp                     mpeg2video              v408
bmv_audio               mpeg4                   v410
bmv_video               mpeg4_cuvid             vb
bonk                    mpegvideo               vble
brender_pix             mpl2                    vbn
c93                     msa1                    vc1
cavs                    mscc                    vc1_cuvid
cbd2_dpcm               msmpeg4v1               vc1_qsv
ccaption                msmpeg4v2               vc1image
cdgraphics              msmpeg4v3               vcr1
cdtoons                 msnsiren                vmdaudio
cdxl                    msp2                    vmdvideo
cfhd                    msrle                   vmix
cinepak                 mss1                    vmnc
clearvideo              mss2                    vnull
cljr                    msvideo1                vorbis
cllc                    mszh                    vp3
comfortnoise            mts2                    vp4
cook                    mv30                    vp5
cpia                    mvc1                    vp6
cri                     mvc2                    vp6a
cscd                    mvdv                    vp6f
cyuv                    mvha                    vp7
dca                     mwsc                    vp8
dds                     mxpeg                   vp8_cuvid
derf_dpcm               nellymoser              vp8_qsv
dfa                     notchlc                 vp9
dfpwm                   nuv                     vp9_cuvid
dirac                   on2avc                  vp9_qsv
dnxhd                   opus                    vplayer
dolby_e                 osq                     vqa
dpx                     paf_audio               vqc
dsd_lsbf                paf_video               vvc
dsd_lsbf_planar         pam                     wady_dpcm
dsd_msbf                pbm                     wavarc
dsd_msbf_planar         pcm_alaw                wavpack
dsicinaudio             pcm_bluray              wbmp
dsicinvideo             pcm_dvd                 wcmv
dss_sp                  pcm_f16le               webp
dst                     pcm_f24le               webvtt
dvaudio                 pcm_f32be               wmalossless
dvbsub                  pcm_f32le               wmapro
dvdsub                  pcm_f64be               wmav1
dvvideo                 pcm_f64le               wmav2
dxa                     pcm_lxf                 wmavoice
dxtory                  pcm_mulaw               wmv1
dxv                     pcm_s16be               wmv2
eac3                    pcm_s16be_planar        wmv3
eacmv                   pcm_s16le               wmv3image
eamad                   pcm_s16le_planar        wnv1
eatgq                   pcm_s24be               wrapped_avframe
eatgv                   pcm_s24daud             ws_snd1
eatqi                   pcm_s24le               xan_dpcm
eightbps                pcm_s24le_planar        xan_wc3
eightsvx_exp            pcm_s32be               xan_wc4
eightsvx_fib            pcm_s32le               xbin
escape124               pcm_s32le_planar        xbm
escape130               pcm_s64be               xface
evrc                    pcm_s64le               xl
exr                     pcm_s8                  xma1
fastaudio               pcm_s8_planar           xma2
ffv1                    pcm_sga                 xpm
ffvhuff                 pcm_u16be               xsub
ffwavesynth             pcm_u16le               xwd
fic                     pcm_u24be               y41p
fits                    pcm_u24le               ylc
flac                    pcm_u32be               yop
flashsv                 pcm_u32le               yuv4
flashsv2                pcm_u8                  zero12v
flic                    pcm_vidc                zerocodec
flv                     pcx                     zlib
fmvc                    pdv                     zmbv
fourxm                  pfm

Enabled encoders:
a64multi                hevc_nvenc              pcm_u24le
a64multi5               hevc_qsv                pcm_u32be
aac                     hevc_vaapi              pcm_u32le
aac_mf                  huffyuv                 pcm_u8
ac3                     jpeg2000                pcm_vidc
ac3_fixed               jpegls                  pcx
ac3_mf                  libaom_av1              pfm
adpcm_adx               libgsm                  pgm
adpcm_argo              libgsm_ms               pgmyuv
adpcm_g722              libmp3lame              phm
adpcm_g726              libopencore_amrnb       png
adpcm_g726le            libopenjpeg             ppm
adpcm_ima_alp           libopus                 prores
adpcm_ima_amv           libspeex                prores_aw
adpcm_ima_apm           libtheora               prores_ks
adpcm_ima_qt            libvo_amrwbenc          qoi
adpcm_ima_ssi           libvorbis               qtrle
adpcm_ima_wav           libvpx_vp8              r10k
adpcm_ima_ws            libvpx_vp9              r210
adpcm_ms                libwebp                 ra_144
adpcm_swf               libwebp_anim            rawvideo
adpcm_yamaha            libx264                 roq
alac                    libx264rgb              roq_dpcm
alias_pix               libx265                 rpza
amv                     libxvid                 rv10
anull                   ljpeg                   rv20
apng                    magicyuv                s302m
aptx                    mjpeg                   sbc
aptx_hd                 mjpeg_qsv               sgi
ass                     mjpeg_vaapi             smc
asv1                    mlp                     snow
asv2                    movtext                 sonic
av1_amf                 mp2                     sonic_ls
av1_nvenc               mp2fixed                speedhq
av1_qsv                 mp3_mf                  srt
av1_vaapi               mpeg1video              ssa
avrp                    mpeg2_qsv               subrip
avui                    mpeg2_vaapi             sunrast
bitpacked               mpeg2video              svq1
bmp                     mpeg4                   targa
cfhd                    msmpeg4v2               text
cinepak                 msmpeg4v3               tiff
cljr                    msrle                   truehd
comfortnoise            msvideo1                tta
dca                     nellymoser              ttml
dfpwm                   opus                    utvideo
dnxhd                   pam                     v210
dpx                     pbm                     v308
dvbsub                  pcm_alaw                v408
dvdsub                  pcm_bluray              v410
dvvideo                 pcm_dvd                 vbn
dxv                     pcm_f32be               vc2
eac3                    pcm_f32le               vnull
exr                     pcm_f64be               vorbis
ffv1                    pcm_f64le               vp8_vaapi
ffvhuff                 pcm_mulaw               vp9_qsv
fits                    pcm_s16be               vp9_vaapi
flac                    pcm_s16be_planar        wavpack
flashsv                 pcm_s16le               wbmp
flashsv2                pcm_s16le_planar        webvtt
flv                     pcm_s24be               wmav1
g723_1                  pcm_s24daud             wmav2
gif                     pcm_s24le               wmv1
h261                    pcm_s24le_planar        wmv2
h263                    pcm_s32be               wrapped_avframe
h263p                   pcm_s32le               xbm
h264_amf                pcm_s32le_planar        xface
h264_mf                 pcm_s64be               xsub
h264_nvenc              pcm_s64le               xwd
h264_qsv                pcm_s8                  y41p
h264_vaapi              pcm_s8_planar           yuv4
hdr                     pcm_u16be               zlib
hevc_amf                pcm_u16le               zmbv
hevc_mf                 pcm_u24be

Enabled hwaccels:
av1_d3d11va             hevc_nvdec              vc1_nvdec
av1_d3d11va2            hevc_vaapi              vc1_vaapi
av1_d3d12va             mjpeg_nvdec             vp8_nvdec
av1_dxva2               mjpeg_vaapi             vp8_vaapi
av1_nvdec               mpeg1_nvdec             vp9_d3d11va
av1_vaapi               mpeg2_d3d11va           vp9_d3d11va2
h263_vaapi              mpeg2_d3d11va2          vp9_d3d12va
h264_d3d11va            mpeg2_d3d12va           vp9_dxva2
h264_d3d11va2           mpeg2_dxva2             vp9_nvdec
h264_d3d12va            mpeg2_nvdec             vp9_vaapi
h264_dxva2              mpeg2_vaapi             wmv3_d3d11va
h264_nvdec              mpeg4_nvdec             wmv3_d3d11va2
h264_vaapi              mpeg4_vaapi             wmv3_d3d12va
hevc_d3d11va            vc1_d3d11va             wmv3_dxva2
hevc_d3d11va2           vc1_d3d11va2            wmv3_nvdec
hevc_d3d12va            vc1_d3d12va             wmv3_vaapi
hevc_dxva2              vc1_dxva2

Enabled parsers:
aac                     dvdsub                  mpegaudio
aac_latm                evc                     mpegvideo
ac3                     flac                    opus
adx                     ftr                     png
amr                     g723_1                  pnm
av1                     g729                    qoi
avs2                    gif                     rv34
avs3                    gsm                     sbc
bmp                     h261                    sipr
cavsvideo               h263                    tak
cook                    h264                    vc1
cri                     hdr                     vorbis
dca                     hevc                    vp3
dirac                   ipu                     vp8
dnxhd                   jpeg2000                vp9
dolby_e                 jpegxl                  vvc
dpx                     misc4                   webp
dvaudio                 mjpeg                   xbm
dvbsub                  mlp                     xma
dvd_nav                 mpeg4video              xwd

Enabled demuxers:
aa                      idcin                   pcm_f64le
aac                     idf                     pcm_mulaw
aax                     iff                     pcm_s16be
ac3                     ifv                     pcm_s16le
ac4                     ilbc                    pcm_s24be
ace                     image2                  pcm_s24le
acm                     image2_alias_pix        pcm_s32be
act                     image2_brender_pix      pcm_s32le
adf                     image2pipe              pcm_s8
adp                     image_bmp_pipe          pcm_u16be
ads                     image_cri_pipe          pcm_u16le
adx                     image_dds_pipe          pcm_u24be
aea                     image_dpx_pipe          pcm_u24le
afc                     image_exr_pipe          pcm_u32be
aiff                    image_gem_pipe          pcm_u32le
aix                     image_gif_pipe          pcm_u8
alp                     image_hdr_pipe          pcm_vidc
amr                     image_j2k_pipe          pdv
amrnb                   image_jpeg_pipe         pjs
amrwb                   image_jpegls_pipe       pmp
anm                     image_jpegxl_pipe       pp_bnk
apac                    image_pam_pipe          pva
apc                     image_pbm_pipe          pvf
ape                     image_pcx_pipe          qcp
apm                     image_pfm_pipe          qoa
apng                    image_pgm_pipe          r3d
aptx                    image_pgmyuv_pipe       rawvideo
aptx_hd                 image_pgx_pipe          realtext
aqtitle                 image_phm_pipe          redspark
argo_asf                image_photocd_pipe      rka
argo_brp                image_pictor_pipe       rl2
argo_cvg                image_png_pipe          rm
asf                     image_ppm_pipe          roq
asf_o                   image_psd_pipe          rpl
ass                     image_qdraw_pipe        rsd
ast                     image_qoi_pipe          rso
au                      image_sgi_pipe          rtp
av1                     image_sunrast_pipe      rtsp
avi                     image_svg_pipe          s337m
avisynth                image_tiff_pipe         sami
avr                     image_vbn_pipe          sap
avs                     image_webp_pipe         sbc
avs2                    image_xbm_pipe          sbg
avs3                    image_xpm_pipe          scc
bethsoftvid             image_xwd_pipe          scd
bfi                     imf                     sdns
bfstm                   ingenient               sdp
bink                    ipmovie                 sdr2
binka                   ipu                     sds
bintext                 ircam                   sdx
bit                     iss                     segafilm
bitpacked               iv8                     ser
bmv                     ivf                     sga
boa                     ivr                     shorten
bonk                    jacosub                 siff
brstm                   jpegxl_anim             simbiosis_imx
c93                     jv                      sln
caf                     kux                     smacker
cavsvideo               kvag                    smjpeg
cdg                     laf                     smush
cdxl                    libgme                  sol
cine                    libopenmpt              sox
codec2                  live_flv                spdif
codec2raw               lmlm4                   srt
concat                  loas                    stl
dash                    lrc                     str
data                    luodat                  subviewer
daud                    lvf                     subviewer1
dcstr                   lxf                     sup
derf                    m4v                     svag
dfa                     matroska                svs
dfpwm                   mca                     swf
dhav                    mcc                     tak
dirac                   mgsts                   tedcaptions
dnxhd                   microdvd                thp
dsf                     mjpeg                   threedostr
dsicin                  mjpeg_2000              tiertexseq
dss                     mlp                     tmv
dts                     mlv                     truehd
dtshd                   mm                      tta
dv                      mmf                     tty
dvbsub                  mods                    txd
dvbtxt                  moflex                  ty
dxa                     mov                     usm
ea                      mp3                     v210
ea_cdata                mpc                     v210x
eac3                    mpc8                    vag
epaf                    mpegps                  vc1
evc                     mpegts                  vc1t
ffmetadata              mpegtsraw               vividas
filmstrip               mpegvideo               vivo
fits                    mpjpeg                  vmd
flac                    mpl2                    vobsub
flic                    mpsub                   voc
flv                     msf                     vpk
fourxm                  msnwc_tcp               vplayer
frm                     msp                     vqf
fsb                     mtaf                    vvc
fwse                    mtv                     w64
g722                    musx                    wady
g723_1                  mv                      wav
g726                    mvi                     wavarc
g726le                  mxf                     wc3
g729                    mxg                     webm_dash_manifest
gdv                     nc                      webvtt
genh                    nistsphere              wsaud
gif                     nsp                     wsd
gsm                     nsv                     wsvqa
gxf                     nut                     wtv
h261                    nuv                     wv
h263                    obu                     wve
h264                    ogg                     xa
hca                     oma                     xbin
hcom                    osq                     xmd
hevc                    paf                     xmv
hls                     pcm_alaw                xvag
hnm                     pcm_f32be               xwma
iamf                    pcm_f32le               yop
ico                     pcm_f64be               yuv4mpegpipe

Enabled muxers:
a64                     h263                    pcm_s24be
ac3                     h264                    pcm_s24le
ac4                     hash                    pcm_s32be
adts                    hds                     pcm_s32le
adx                     hevc                    pcm_s8
aea                     hls                     pcm_u16be
aiff                    iamf                    pcm_u16le
alp                     ico                     pcm_u24be
amr                     ilbc                    pcm_u24le
amv                     image2                  pcm_u32be
apm                     image2pipe              pcm_u32le
apng                    ipod                    pcm_u8
aptx                    ircam                   pcm_vidc
aptx_hd                 ismv                    psp
argo_asf                ivf                     rawvideo
argo_cvg                jacosub                 rcwt
asf                     kvag                    rm
asf_stream              latm                    roq
ass                     lrc                     rso
ast                     m4v                     rtp
au                      matroska                rtp_mpegts
avi                     matroska_audio          rtsp
avif                    md5                     sap
avm2                    microdvd                sbc
avs2                    mjpeg                   scc
avs3                    mkvtimestamp_v2         segafilm
bit                     mlp                     segment
caf                     mmf                     smjpeg
cavsvideo               mov                     smoothstreaming
codec2                  mp2                     sox
codec2raw               mp3                     spdif
crc                     mp4                     spx
dash                    mpeg1system             srt
data                    mpeg1vcd                stream_segment
daud                    mpeg1video              streamhash
dfpwm                   mpeg2dvd                sup
dirac                   mpeg2svcd               swf
dnxhd                   mpeg2video              tee
dts                     mpeg2vob                tg2
dv                      mpegts                  tgp
eac3                    mpjpeg                  truehd
evc                     mxf                     tta
f4v                     mxf_d10                 ttml
ffmetadata              mxf_opatom              uncodedframecrc
fifo                    null                    vc1
filmstrip               nut                     vc1t
fits                    obu                     voc
flac                    oga                     vvc
flv                     ogg                     w64
framecrc                ogv                     wav
framehash               oma                     webm
framemd5                opus                    webm_chunk
g722                    pcm_alaw                webm_dash_manifest
g723_1                  pcm_f32be               webp
g726                    pcm_f32le               webvtt
g726le                  pcm_f64be               wsaud
gif                     pcm_f64le               wtv
gsm                     pcm_mulaw               wv
gxf                     pcm_s16be               yuv4mpegpipe
h261                    pcm_s16le

Enabled protocols:
async                   http                    rtmp
cache                   httpproxy               rtmpe
concat                  https                   rtmps
concatf                 icecast                 rtmpt
crypto                  ipfs_gateway            rtmpte
data                    ipns_gateway            rtmpts
fd                      libsrt                  rtp
ffrtmpcrypt             libssh                  srtp
ffrtmphttp              libzmq                  subfile
file                    md5                     tcp
ftp                     mmsh                    tee
gopher                  mmst                    tls
gophers                 pipe                    udp
hls                     prompeg                 udplite

Enabled filters:
a3dscope                datascope               palettegen
aap                     dblur                   paletteuse
abench                  dcshift                 pan
abitscope               dctdnoiz                perms
acompressor             ddagrab                 perspective
acontrast               deband                  phase
acopy                   deblock                 photosensitivity
acrossfade              decimate                pixdesctest
acrossover              deconvolve              pixelize
acrusher                dedot                   pixscope
acue                    deesser                 pp
addroi                  deflate                 pp7
adeclick                deflicker               premultiply
adeclip                 deinterlace_qsv         prewitt
adecorrelate            deinterlace_vaapi       procamp_vaapi
adelay                  dejudder                pseudocolor
adenorm                 delogo                  psnr
aderivative             denoise_vaapi           pullup
adrawgraph              derain                  qp
adrc                    deshake                 random
adynamicequalizer       despill                 readeia608
adynamicsmooth          detelecine              readvitc
aecho                   dialoguenhance          realtime
aemphasis               dilation                remap
aeval                   displace                removegrain
aevalsrc                dnn_classify            removelogo
aexciter                dnn_detect              repeatfields
afade                   dnn_processing          replaygain
afdelaysrc              doubleweave             reverse
afftdn                  drawbox                 rgbashift
afftfilt                drawgraph               rgbtestsrc
afir                    drawgrid                roberts
afireqsrc               drawtext                rotate
afirsrc                 drmeter                 rubberband
aformat                 dynaudnorm              sab
afreqshift              earwax                  scale
afwtdn                  ebur128                 scale2ref
agate                   edgedetect              scale_cuda
agraphmonitor           elbg                    scale_qsv
ahistogram              entropy                 scale_vaapi
aiir                    epx                     scdet
aintegral               eq                      scharr
ainterleave             equalizer               scroll
alatency                erosion                 segment
alimiter                estdif                  select
allpass                 exposure                selectivecolor
allrgb                  extractplanes           sendcmd
allyuv                  extrastereo             separatefields
aloop                   fade                    setdar
alphaextract            feedback                setfield
alphamerge              fftdnoiz                setparams
amerge                  fftfilt                 setpts
ametadata               field                   setrange
amix                    fieldhint               setsar
amovie                  fieldmatch              settb
amplify                 fieldorder              sharpness_vaapi
amultiply               fillborders             shear
anequalizer             find_rect               showcqt
anlmdn                  firequalizer            showcwt
anlmf                   flanger                 showfreqs
anlms                   floodfill               showinfo
anoisesrc               format                  showpalette
anull                   fps                     showspatial
anullsink               framepack               showspectrum
anullsrc                framerate               showspectrumpic
apad                    framestep               showvolume
aperms                  freezedetect            showwaves
aphasemeter             freezeframes            showwavespic
aphaser                 fspp                    shuffleframes
aphaseshift             fsync                   shufflepixels
apsnr                   gblur                   shuffleplanes
apsyclip                geq                     sidechaincompress
apulsator               gradfun                 sidechaingate
arealtime               gradients               sidedata
aresample               graphmonitor            sierpinski
areverse                grayworld               signalstats
arls                    greyedge                signature
arnndn                  guided                  silencedetect
asdr                    haas                    silenceremove
asegment                haldclut                sinc
aselect                 haldclutsrc             sine
asendcmd                hdcd                    siti
asetnsamples            headphone               smartblur
asetpts                 hflip                   smptebars
asetrate                highpass                smptehdbars
asettb                  highshelf               sobel
ashowinfo               hilbert                 spectrumsynth
asidedata               histeq                  speechnorm
asisdr                  histogram               split
asoftclip               hqdn3d                  spp
aspectralstats          hqx                     sr
asplit                  hstack                  ssim
ass                     hstack_qsv              ssim360
astats                  hstack_vaapi            stereo3d
astreamselect           hsvhold                 stereotools
asubboost               hsvkey                  stereowiden
asubcut                 hue                     streamselect
asupercut               huesaturation           subtitles
asuperpass              hwdownload              super2xsai
asuperstop              hwmap                   superequalizer
atadenoise              hwupload                surround
atempo                  hwupload_cuda           swaprect
atilt                   hysteresis              swapuv
atrim                   identity                tblend
avectorscope            idet                    telecine
avgblur                 il                      testsrc
avsynctest              inflate                 testsrc2
axcorrelate             interlace               thistogram
azmq                    interleave              threshold
backgroundkey           join                    thumbnail
bandpass                kerndeint               thumbnail_cuda
bandreject              kirsch                  tile
bass                    lagfun                  tiltandshift
bbox                    latency                 tiltshelf
bench                   lenscorrection          tinterlace
bilateral               libvmaf                 tlut2
bilateral_cuda          life                    tmedian
biquad                  limitdiff               tmidequalizer
bitplanenoise           limiter                 tmix
blackdetect             loop                    tonemap
blackframe              loudnorm                tonemap_vaapi
blend                   lowpass                 tpad
blockdetect             lowshelf                transpose
blurdetect              lumakey                 transpose_vaapi
bm3d                    lut                     treble
boxblur                 lut1d                   tremolo
bwdif                   lut2                    trim
bwdif_cuda              lut3d                   unpremultiply
cas                     lutrgb                  unsharp
ccrepack                lutyuv                  untile
cellauto                mandelbrot              uspp
channelmap              maskedclamp             v360
channelsplit            maskedmax               vaguedenoiser
chorus                  maskedmerge             varblur
chromahold              maskedmin               vectorscope
chromakey               maskedthreshold         vflip
chromakey_cuda          maskfun                 vfrdet
chromanr                mcdeint                 vibrance
chromashift             mcompand                vibrato
ciescope                median                  vidstabdetect
codecview               mergeplanes             vidstabtransform
color                   mestimate               vif
colorbalance            metadata                vignette
colorchannelmixer       midequalizer            virtualbass
colorchart              minterpolate            vmafmotion
colorcontrast           mix                     volume
colorcorrect            monochrome              volumedetect
colorhold               morpho                  vpp_qsv
colorize                movie                   vstack
colorkey                mpdecimate              vstack_qsv
colorlevels             mptestsrc               vstack_vaapi
colormap                msad                    w3fdif
colormatrix             multiply                waveform
colorspace              negate                  weave
colorspace_cuda         nlmeans                 xbr
colorspectrum           nnedi                   xcorrelate
colortemperature        noformat                xfade
compand                 noise                   xmedian
compensationdelay       normalize               xstack
concat                  null                    xstack_qsv
convolution             nullsink                xstack_vaapi
convolve                nullsrc                 yadif
copy                    oscilloscope            yadif_cuda
corr                    overlay                 yaepblur
cover_rect              overlay_cuda            yuvtestsrc
crop                    overlay_qsv             zmq
cropdetect              overlay_vaapi           zoneplate
crossfeed               owdenoise               zoompan
crystalizer             pad                     zscale
cue                     pal100bars
curves                  pal75bars

Enabled bsfs:
aac_adtstoasc           h264_redundant_pps      pgs_frame_merge
av1_frame_merge         hapqa_extract           prores_metadata
av1_frame_split         hevc_metadata           remove_extradata
av1_metadata            hevc_mp4toannexb        setts
chomp                   imx_dump_header         showinfo
dca_core                media100_to_mjpegb      text2movsub
dts2pts                 mjpeg2jpeg              trace_headers
dump_extradata          mjpega_dump_header      truehd_core
dv_error_marker         mov2textsub             vp9_metadata
eac3_core               mpeg2_metadata          vp9_raw_reorder
evc_frame_merge         mpeg4_unpack_bframes    vp9_superframe
extract_extradata       noise                   vp9_superframe_split
filter_units            null                    vvc_metadata
h264_metadata           opus_metadata           vvc_mp4toannexb
h264_mp4toannexb        pcm_rechunk

Enabled indevs:
dshow                   lavfi
gdigrab                 vfwcap

Enabled outdevs:
sdl2

release-essentials external libraries' versions: 

AMF v1.4.32-14-ge1acd43
aom v3.9.0-157-g6f8189bb64
AviSynthPlus v3.7.3-70-g2b55ba40
ffnvcodec n12.2.72.0-1-g9934f17
freetype VER-2-13-2
fribidi v1.0.14
gsm 1.0.22
harfbuzz 8.5.0-123-gee0c7d6b
lame 3.100
libass 0.17.2-6-g85ff2b3
libgme 0.6.3
libopencore-amrnb 0.1.6
libopencore-amrwb 0.1.6
libssh 0.10.6
libtheora 1.1.1
libwebp v1.4.0-17-g45129ee0
oneVPL 2.11
openmpt libopenmpt-0.6.16-3-gef06a8e61
opus v1.5.2-11-g2554a89e
rubberband v1.8.1
SDL prerelease-2.29.2-185-gc79e61680
speex Speex-1.2.1-25-g117bcc0
srt v1.5.3-79-g38a3a16
VAAPI 2.22.0.
vidstab v1.1.1-11-gc8caf90
vmaf v3.0.0-81-gb24c3d68
vo-amrwbenc 0.1.3
vorbis v1.3.7-10-g84c02369
vpx v1.14.0-282-g495c4b596
x264 v0.164.3191
x265 3.6-28-g8787e8702
xvid v1.3.7
zeromq 4.3.5
zimg release-3.0.5-150-g7143181

