import React from "react";

const MsgContext = React.createContext();
const UserNamePattern = /^[a-zA-Z]{1,30}$/g;
const PasswordPattern = /^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,30}$/g;
const UserNameNotValidMsg = "שם משתמש לא חוקי. חייב לכלול רק אותיות באנגלית.";
const PasswordNotValidMsg = "סיסמה לא חוקית. חייבת להיות בין 8 ל 30 תווים מורכבת מאותיות גדולות וקטנות באנגלית וספרות.";
const apiUrl = "http://localhost:54996/api";
//const apiUrl = "../api";

export { MsgContext, UserNamePattern, PasswordPattern, UserNameNotValidMsg, PasswordNotValidMsg, apiUrl };

export function dateRender(row, data, index) { return (row.CreationDate.split("T")[0].split("-").reverse().join(".")); }

export function GMT0(dateI) {
  let br;
  let br2 = [];
  if (dateI != null) {
    br = dateI.toString().split(" ");
    for (let i = 0; i <= 5; i++) { br2.push(br[i]); }
    br2[5] = "GMT+0000";
    return new Date(br2.join(" "));
  }
  else return null;
};

export function fetchCall(method, api, data, successCB, errorCB, token = "", userName = "") {
  let metBodyHead = {
    method: method,
    headers: new Headers({
      'Access-Control-Allow-Origin': '*',
      'Content-Type': 'application/json; charset=UTF-8',
      "X-Token": token,
      "X-UserName": userName
    }),
  }
  if (data != "") metBodyHead.body = JSON.stringify(data);
  fetch(api, metBodyHead)
    .then((resp) => {
      if (!resp.ok) {
        let err = new Error();
        err.resp = resp;
        throw err;
      }
      return resp.json();
    })
    .then(data => successCB(data))
    .catch(error => {
      if (error.Message == undefined) error.Message = "error";
      if (error.resp != undefined) error.resp.json().then(error => errorCB(error));
      else errorCB(error);
    })
};