"use strict";
const nodemailer = require("nodemailer");

async function main() {
  let transporter = nodemailer.createTransport({
    host: "smtp.gmail.com",
    port: 587,
    secure: false, 
    auth: {
      user: "ant.adons@gmail.com", 
      pass: "tmgjkqhhgukgarat", 
    },
  });
  
  let info = await transporter.sendMail({
    from: '"Антон Владимирович" <ant.adons@gmail.com>', 
    to: "anton.pikin9@gmail.com", 
    subject: "Диплом", 
    html: "<b style='color:red'>Здравствуйте, Евгений Олегович,"+
    " я студент группы М4О-414Б-17 Пикин Антон. интересуюсь разработкой"+
    " веб приложений. Не могли бы вы посоветовать"+
    " подходящую тему для дипломной работы?</b>"+
    "<img src='cid:logo'>",
    attachments:[{
      filename: 'ping.png',
      path: __dirname +'/ping.png',
      cid: 'logo'
    }]
  });

  console.log("Message sent: %s", info.messageId);
}

main().catch(console.error);