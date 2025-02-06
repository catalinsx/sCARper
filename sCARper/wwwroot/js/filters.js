document.getElementById("applyFilters").addEventListener("click", function () {
   
    //getting filters values    
    const fuelType = document.getElementById("fuelType").value;
    const year = document.getElementById("year").value;
    const mileage = document.getElementById("mileage").value;
    const maxPrice = document.getElementById("maxPrice").value;

    // this creates a new urlsearchparams object that helps to manipulate query parameters in the url
    const queryParams = new URLSearchParams();

    // these lines check if each filter value exists if so adds it to the queryparams
    if (fuelType) queryParams.append("fuelType", fuelType);
    if (year) queryParams.append("minYear", year);
    if (mileage) queryParams.append("maxMileage", mileage);
    if (maxPrice) queryParams.append("maxPrice", maxPrice);
    
    
    // this retrieves the current page number from the urls query parameters using window.location.search
    // the page number is added to the query params object to ensure the user stays on teh same page after applying the filters
    const page = new URLSearchParams(window.location.search).get("page") || 1;
    queryParams.append("page", page);

    // building the url
    const baseUrl = window.location.origin + "/Result/ShowResults";
    const fullUrl = `${baseUrl}?${queryParams.toString()}`;

    window.location.href = fullUrl;
});


// this function runs when the page is fully loaded
// it retrieves the current query parameters from url using urlsearchparams
// it then checks if any filter value are present in the url
// if a filter value exists it sets the corresponding input field's value tto match the url parameter
window.onload = function () {
    const urlParams = new URLSearchParams(window.location.search);

    const fuelType = urlParams.get("fuelType");
    const year = urlParams.get("minYear");
    const mileage = urlParams.get("maxMileage");
    const maxPrice = urlParams.get("maxPrice");

    if (fuelType) document.getElementById("fuelType").value = fuelType;
    if (year) document.getElementById("year").value = year;
    if (mileage) document.getElementById("mileage").value = mileage;
    if (maxPrice) document.getElementById("maxPrice").value = maxPrice;
};
