var DeviceInfo = {
    // Function to get user agent string
    GetUserAgentJS: function() {
    var returnStr = navigator.userAgent;
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    },
}
autoAddDeps(DeviceInfo,'GetUserAgentJS')
mergeInto(LibraryManager.library, DeviceInfo);