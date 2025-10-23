////var ezgocachefixer = {
////    removeAllCorsErrorCacheItems: async function () {
////        let needsReload = false;
////        var images = [];
////        $("img").each(function () {
////            if (this.src) {
////                images.push(this.src);
////            }
////        });

////        for (let i = 0; i < images.length; i++) {
////            needsReload |= await ezgocachefixer.tryRemoveInvalidCache(images[i]);
////        }
////        if (needsReload) {
////            window.location.reload(true);
////        }
////    },

////    tryRemoveInvalidCache: async function (pictureUrl) {
////        if (!pictureUrl.startsWith(ezgolist.mediaUrl)) {
////            return false;
////        }
////        var response = {};
////        try {
////            response = await fetch(pictureUrl, { 'mode': 'cors' });
////            if (response.status == 200) {
////                return false;
////            }
////        }
////        catch (error) {
////            caches.open('v1').then(function (cache) {
////                cache.delete(pictureUrl).then(function (result) {
////                    console.log(`Picture ${pictureUrl} cleared from cache`);
////                });
////            });

////            response = await fetch(pictureUrl, {
////                'mode': 'cors',
////                headers: {
////                    'Cache-Control': 'no-cache'
////                }
////            });
////            return true;
////        }
////    }
////};