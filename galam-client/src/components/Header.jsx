import React from 'react'

export default function Header(props) {

  return (
    <div className={(props.logedIn && !props.MustChangePassword ? "head" : "head headWoMenu")}>
      {(props.logedIn ?
        <div className="headR">
          משתמש: {props.userName}<br />
          <button className="blueButton" id="butExit" onClick={props.LogOut}>יציאה</button>
        </div>
        : <div />)}
      <div className="brand">תוכנת תחזיות</div>
      <img className="logo" src="./img/logo.png" />
    </div>
  )
}
