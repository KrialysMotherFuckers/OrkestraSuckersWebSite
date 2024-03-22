// ****************************************
// IMPLEMENT YOUR INTEROP FUNCTIONS HERE \\
// ****************************************

/**
 * Get browser inner height.
 * 
 * @return Browser inner height (in px).
 */
function getBrowserHeight() {
    return window.innerHeight;
};

/**
 * Get element top position relative to the viewport (in px).
 *
 * @param {string} elementId Id of the targeted element.
 * @return Top position relative to the viewport (in px).
 */
function getElementPosition(elementId) {

    // Get element based on it ID.
    const element = document.getElementById(elementId);

    // Return element top position relative to the viewport (in px).
    if (element !== null) {
        return element.getBoundingClientRect().top;
    }
    else {
        //console.warn('⛔️ Element ' + elementId + ' does NOT exist in DOM');
        return 0;
    }
}

/**
 * Get the offsetTop position of an HTML element
 *
 * @param {string} elementId Id of the targeted element.
 * @return Offset Top position relative to the viewport (in px).
 */
function getElementOffsetTop(elementId, withinTab) {
    // Get element based on it ID.
    const element = document.getElementById(elementId);

    // Return element top position relative to the viewport (in px).
    if (element !== null) {
        return Math.max(element.offsetTop, element.offsetParent?.offsetTop, element.offsetParent?.offsetParent.offsetTop);
    }
    else {
        //console.warn('⛔️ Element ' + elementId + ' does NOT exist in DOM');
        return defaultValue;
    }
}

/**
 * Observe a target element.
 * When element is visble, execute "OnIntersection" C# callback.
 *
 * @param {object} dotNetObjectReference Dot net object instance.
 * @param {Element} observerTargetId Id of the observed element.
 * @return Dispose function.
 */
function setObserver(dotNetObjectReference, observerTargetId) {
    // Get element to observe based on its ID.
    let observerTarget = document.getElementById(observerTargetId);
    if (observerTarget == null) throw new Error("Observer target was not found");

    // Create intersection observer.
    const observer = new IntersectionObserver(async (entries) => {
        // Callback function executed when targeted element is visible.
        for (const entry of entries) {
            if (entry.isIntersecting) {
                await dotNetObjectReference.invokeMethodAsync('OnIntersection');
            }
        }
    });

    // Tells the target to observe.
    observer.observe(observerTarget);

    // Return dispose function.
    return {
        dispose: () => {
            // Stop oberving.
            observer.disconnect();
        }
    };
}


/**
 * Download file
 * Use it for .NET 6+
 * Link: https://www.meziantou.net/generating-and-downloading-a-file-in-a-blazor-webassembly-application.htm
 * @param {string} filename file name to download (i.e.: 'file.bin')
 * @param {string} contentType content type (i.e.: 'application/octet-stream', 'text/plain'...)
 * @param {byte[]} bytesBase64 content of the file
 * @return Dispose function.
 */
function downloadFile(filename, contentType, bytesBase64) {
    if (contentType === "") {
        contentType = 'application/octet-stream';
    }
    // Create the URL
    const file = new File([bytesBase64], filename, { type: contentType });
    const exportUrl = URL.createObjectURL(file);

    // Create the <a> element and click on it
    const a = document.createElement("a");
    document.body.appendChild(a);
    a.href = exportUrl;
    a.download = filename;
    a.target = "_self";
    a.click();

    // We don't need to keep the object URL, let's release the memory
    // On older versions of Safari, it seems you need to comment this line...
    URL.revokeObjectURL(exportUrl);
}

/**
 * Send click onto an HTML element datagrid id
 * Use it for .NET 6+
 * @param {string} id id of the HTML element
 * @param {int} timeout
 * @return object
 */
function sendClickToRefreshDatagrid(id, timeout) {
    setTimeout(function () {
        // Get element based on it ID.
        const element = document.getElementById(id);
        if (element !== null) {
            element.click();
        }
    }, timeout);
}

/**
* JavaScript code to invoke a C# method after some delay
* @param {object} dotNetRef dotnet reference
* @param {string} methodName method to invoke
* @param {int} timeout
* @return object 
*/
function invokeCsharpCallback(dotNetRef, methodName, timeout) {
    setTimeout(function () {
        dotNetRef.invokeMethodAsync(methodName);
    }, timeout);
}