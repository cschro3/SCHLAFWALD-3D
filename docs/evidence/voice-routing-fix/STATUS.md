# Emma routing and stale-cache repair

User reported good Emma audio in chat but robotic audio in the app. The previous code allowed any saved nonempty voiceURI to replace bundled Emma with Web Speech. A regression test reproduced source=device instead of source=emma-preview-recording before the fix.

Visit recordings now always take priority, regardless of saved device voice. Missing visit recordings show the read-aloud fallback instead of automatically synthesizing a robot voice. Device voice controls in the parent area are labelled as comparison auditions. MP3 playback is explicitly normal speed and preserves pitch.

Recording metadata is bundled in the hashed app JavaScript, so an old service worker cannot supply an empty cached visit manifest. Generated builds include emma.html as a fresh entry path. New service workers request fresh navigation pages when online, retaining cached offline fallback. No progress or settings are deleted. No new speech generation or credits consumed.

Executed: 42/42 Node tests; 3/3 targeted real browser tests (device audition controls, real offline Emma playback to actual end, and real MP3 despite a saved robot voice); 1/1 additional browser test installs a legacy cache-first service worker with an old start page and empty audio manifest, then confirms the fresh Emma entry opens and plays the real MP3. Reports are adjacent XMLs. Production build succeeded, 195 offline assets.

The selected Suri take2 in the app is byte-identical to the approved original. Original vs normalized Suri02 decoded waveforms have correlation0.99444 and identical9.44second duration. This sample comparison does not establish subjective listening quality of all82 clips or the user's actual cached installation.

The web preview gets the fix upon deployment. Previously downloaded APKs contain the older code and require a new Android build. New CI builds are reported separately. No real-device test claimed.
