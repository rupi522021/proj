import React from 'react'

export default function ModalWidow(props) {
  return (
    <div className="onallout" >
      <div className="onalloin" >
        <div className="xx" onClick={props.cancelX}>X</div>
        {props.header != undefined ? <div><h1>{props.header}</h1></div> : ""}
        {props.children}
      </div >
    </div >
  )
}