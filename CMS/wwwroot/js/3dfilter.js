var threeDFilter = {};

// read the localstorage
var strJson = localStorage.getItem('3dfilters');

if (strJson !== null) {
    threeDFilter = JSON.parse(strJson);
}

// 
function changeFilters(filtergroup, filterarray) {

    var arr2 = getFilterGroup(filtergroup);

    // compare both array and send all differences from both sides
    let difference = filterarray
        .filter(x => !arr2.includes(x))
        .concat(arr2.filter(x => !filterarray.includes(x)));

    $(difference).each(function (index, filterkey) {
        changeFilter(filtergroup, filterkey);
    });

}

function changeFilter(groupkey, filtervalue) {

    if (threeDFilter === 'undefined') {
        // read the localstorage
        var strJson = localStorage.getItem('3dfilters');

        if (strJson !== null) {
            threeDFilter = JSON.parse(strJson);
        }
    }

    // check if we have the groupkey, if not add empty array for the groupkey
    var filterGroup = threeDFilter[groupkey];
    if (!filterGroup) {
        filterGroup = threeDFilter[groupkey] = [];
    }

    filtervalue = filtervalue.toString();
    //add the filtervalue or remove if already present
    if (filtervalue !== '*') {

        // add or remone the filtervalue to or from the group
        var index = jQuery.inArray(filtervalue, filterGroup);

        if (index === -1) {
            threeDFilter[groupkey].push(filtervalue);
        }
        else {
            threeDFilter[groupkey].splice(index, 1);
        }

    } else {
        delete threeDFilter[groupkey];
        //threeDFilter[groupkey] = [];
    }

    // cleanup empty group-arrays
    $.each(threeDFilter, function (key, value) {
        var grouparray = threeDFilter[key];
        if (Array.isArray(grouparray) && grouparray.length) {
            // the array is defined and has at least one element
        } else {
            delete threeDFilter[key];
        }
    });

    // save in localstorage
    localStorage.setItem('3dfilters', JSON.stringify(threeDFilter));
}

function checkItemInFilter(item) {

    if (threeDFilter === 'undefined') {
        // read the localstorage
        var strJson = localStorage.getItem('3dfilters');

        if (strJson !== null) {
            threeDFilter = JSON.parse(strJson);
        }
    }

    var arr = [];
    for (var key in threeDFilter) {
        var count = 0;
        for (var subkey in threeDFilter[key]) {
            if (item.data(key)) {
                var keyvalue = threeDFilter[key][subkey];
                var myfilter = item.data(key);
                if (myfilter !== null) {
                    if (typeof myfilter !== 'string') { myfilter = myfilter.toString();}
                    var items = myfilter.split(',');
                    if (items.some(function (v) { return v === keyvalue; })) {
                        count++;
                    }
                }
            }
        }
        arr.push(count);
    }

    var result = true;
    for (var el in arr) {
        if (arr[el] === 0) { result = false; }
    }

    return result;
}

function checkFilterActive(item) {

    if (threeDFilter === 'undefined') {
        // read the localstorage
        var strJson = localStorage.getItem('3dfilters');

        if (strJson !== null) {
            threeDFilter = JSON.parse(strJson);
        }
    }

    for (var groupkey in threeDFilter) {
        if (item.data(groupkey)) {
            var myfilter = item.data(groupkey);
            if (myfilter !== null) {
                if (typeof myfilter != 'string') { myfilter = myfilter.toString(); }
                var items = threeDFilter[groupkey];
                if (items.some(function (v) { return v === myfilter; })) {
                    return true;
                }
            }
        }
    }

    return false;
}

function resetFilters(filters) {

    if (filters.length) {
        var items = filters.split(/\s*,\s*/);
        $.each(threeDFilter, function (key, value) {
            if (items.some(function (v) { return v === key; })) {
                delete threeDFilter[key];
            }
        });
        localStorage.setItem('3dfilters', JSON.stringify(threeDFilter));
    } else {
        // remove all filters
        localStorage.removeItem('3dfilters');
    }

}

function getFilterGroup(groupkey) {

    var strJson = localStorage.getItem('3dfilters');

    if (strJson !== null) {
        threeDFilter = JSON.parse(strJson);
    }

    let arr = threeDFilter[groupkey];
    if (!Array.isArray(arr)) {
        arr = [];
    }

    return arr;
}